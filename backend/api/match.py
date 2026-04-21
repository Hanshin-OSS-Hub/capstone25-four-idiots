import logging
from uuid import uuid4

from flask import Blueprint, request, g
from sqlalchemy import text

from common.responses import fail, ok
from database import get_db
from .auth import require_user
from services.runtime_state import create_arena_match, get_arena_match, pop_arena_match, save_arena_match
from services.tier_service import tier_name_to_api

bp = Blueprint("match", __name__, url_prefix="/v1/match")
logger = logging.getLogger(__name__)

MAX_ARENA_LIVES = 4
MAX_ARENA_TIME_MS = 60000


def _internal_error_response(action):
    logger.exception("Match API error during %s", action)
    return fail("INTERNAL_ERROR", "Unexpected server error", 500)


def _load_match_for_user(match_id, user_id):
    match = get_arena_match(match_id)
    if not match:
        return None, fail("NOT_FOUND", "match not found", 404)
    if match.get("user_id") != user_id:
        return None, fail("FORBIDDEN", "not your match", 403)
    return match, None

from services.arena_service import (
    CATEGORY_CP_COLUMNS,
    CATEGORY_DISPLAY_NAMES,
    delete_arena_progress as _delete_arena_progress,
    load_arena_progress as _load_arena_progress,
    load_my_profile as _load_my_profile,
    load_opponent_set as _load_opponent_set,
    resolve_round as _resolve_round,
    reward_for_result as _reward_for_result,
    rotate_records as _rotate_records,
    score_answer as _score_answer,
    update_arena_profile as _update_arena_profile,
    upsert_arena_progress as _upsert_arena_progress,
)
from services.question_service import (
    load_question_detail as _load_question_detail,
    normalize_category as _normalize_category,
    prepare_question_for_delivery as _prepare_question_for_delivery,
)


@bp.post("/find")
@require_user
def find_match():
    try:
        db = get_db()
        data = request.get_json(silent=True) or {}
        category = _normalize_category(data.get("category"))

        if not category:
            return fail("BAD_REQUEST", "category is required", 400)

        user_id = g.get("user_id")
        cp_column = CATEGORY_CP_COLUMNS[category]
        category_name = CATEGORY_DISPLAY_NAMES[category]

        me = _load_my_profile(db, user_id, cp_column)
        if not me:
            return fail("USER_NOT_FOUND", "profile not found", 404)

        candidates_query = text(
            f"""
            SELECT
                rbm.user_id AS opponent_id,
                u.nickname,
                p.tier_name,
                p.arena_rating,
                p.{cp_column} AS opponent_power,
                rbm.set_id,
                rbm.updated_cp,
                ABS(rbm.updated_cp - :my_power) AS cp_gap,
                rbm.created_at
            FROM RECORD_BATTLE_MATCH rbm
            JOIN USER u ON u.user_id = rbm.user_id
            JOIN PROFILE p ON p.user_id = rbm.user_id
            WHERE rbm.category_name = :category_name
              AND rbm.user_id <> :user_id
            ORDER BY cp_gap ASC, rbm.created_at DESC
            LIMIT 10
            """
        )
        rows = db.execute(
            candidates_query,
            {
                "category_name": category_name,
                "user_id": user_id,
                "my_power": int(me["my_power"] or 0),
            },
        ).mappings().all()

        candidates = []
        for row in rows:
            match_id = f"match-{uuid4().hex[:8]}"
            room_id = f"room-{uuid4().hex[:8]}"
            create_arena_match(match_id, {
                "match_id": match_id,
                "room_id": room_id,
                "user_id": user_id,
                "category": category,
                "set_id": row["set_id"],
                "updated_cp": int(row["updated_cp"] or 0),
                "opponent_id": row["opponent_id"],
                "opponent_nickname": row["nickname"],
                "my_lives": MAX_ARENA_LIVES,
                "opponent_lives": MAX_ARENA_LIVES,
                "my_score": 0,
                "opponent_score": 0,
                "questions": {},
                "question_order": [],
                "question_number_map": {},
                "answered": set(),
                "cp_gap": int(row["cp_gap"] or 0),
            })
            candidates.append(
                {
                    "match_id": match_id,
                    "room_id": room_id,
                    "opponent": {
                        "id": row["opponent_id"],
                        "nickname": row["nickname"],
                        "tier": tier_name_to_api(row["tier_name"]),
                        "arena_rating": int(row["arena_rating"] or 0),
                        "power": int(row["opponent_power"] or 0),
                    },
                    "set_id": row["set_id"],
                    "cp_gap": int(row["cp_gap"] or 0),
                }
            )

        return ok(
            {
                "category": category,
                "my_profile": {
                    "user_id": me["user_id"],
                    "nickname": me["nickname"],
                    "tier": tier_name_to_api(me["tier_name"]),
                    "arena_rating": int(me["arena_rating"] or 0),
                    "power": int(me["my_power"] or 0),
                },
                "candidates": candidates,
                "status": "matched" if candidates else "empty",
            }
        )
    except Exception:
        return _internal_error_response("find_match")


@bp.post("/start")
@require_user
def start_battle():
    try:
        db = get_db()
        data = request.get_json(silent=True) or {}
        match_id = data.get("match_id")

        if not match_id:
            return fail("BAD_REQUEST", "match_id is required", 400)

        match, error_response = _load_match_for_user(match_id, g.get("user_id"))
        if error_response:
            return error_response

        progress = _load_arena_progress(db, match["user_id"], match["opponent_id"], match["category"])
        if progress and progress["set_id"] != match["set_id"]:
            progress = None

        records = _load_opponent_set(db, match["set_id"], match["category"])
        if not records:
            return fail("NOT_FOUND", "opponent record set not found", 404)

        resume_order = int(progress["last_question_order"] or 0) if progress else 0
        rotated_records = _rotate_records(records, resume_order)

        safe_questions = []
        question_map = {}
        question_order = []
        question_number_map = {}
        opponent_total_score = 0

        for record in rotated_records:
            row = _load_question_detail(db, match["category"], record["q_id"])
            if not row:
                continue
            question_id = row["q_id"]
            payload, correct_answer = _prepare_question_for_delivery(match["category"], row)
            question_map[question_id] = {
                "correct_answer": correct_answer,
                "score": int(row["score"] or 0),
                "opponent_time_sec": int(record["solve_time_sec"] or 0),
                "opponent_correct": bool(record["is_correct"]),
                "question_order_number": int(record["question_order_number"] or 0),
            }
            question_number_map[question_id] = int(record["question_order_number"] or 0)
            question_order.append(question_id)
            safe_questions.append(payload)
            if bool(record["is_correct"]):
                opponent_total_score += int(row["score"] or 0)

        if not safe_questions:
            return fail("NOT_FOUND", "opponent questions not found", 404)

        match["questions"] = question_map
        match["question_order"] = question_order
        match["question_number_map"] = question_number_map
        match["opponent_score"] = opponent_total_score
        save_arena_match(match_id, match)

        _upsert_arena_progress(db, match, resume_order)

        return ok(
            {
                "match_id": match_id,
                "room_id": match["room_id"],
                "category": match["category"],
                "round_time": 60,
                "my_lives": match["my_lives"],
                "opponent_lives": match["opponent_lives"],
                "resumed": progress is not None,
                "resume_from_order": resume_order,
                "questions": safe_questions,
            }
        )
    except Exception:
        return _internal_error_response("start_battle")


@bp.post("/submit")
@require_user
def submit_battle_answer():
    try:
        db = get_db()
        data = request.get_json(silent=True) or {}
        match_id = data.get("match_id")
        question_id = data.get("question_id")
        submitted_answer = data.get("answer", data.get("choice"))
        time_ms = int(data.get("time_ms", 0))

        if not match_id or not question_id:
            return fail("BAD_REQUEST", "match_id and question_id are required", 400)

        match, error_response = _load_match_for_user(match_id, g.get("user_id"))
        if error_response:
            return error_response
        if question_id in match["answered"]:
            return fail("BAD_REQUEST", "question already submitted", 400)

        question = match["questions"].get(question_id)
        if not question:
            return fail("NOT_FOUND", "question not found", 404)

        player_correct, timed_out, earned_score = _score_answer(question, submitted_answer, time_ms)
        round_result = _resolve_round(
            player_correct,
            time_ms,
            question["opponent_correct"],
            question["opponent_time_sec"],
        )

        if round_result == "player_attack":
            match["opponent_lives"] = max(0, match["opponent_lives"] - 1)
        elif round_result == "opponent_attack":
            match["my_lives"] = max(0, match["my_lives"] - 1)

        match["my_score"] += earned_score
        match["answered"].add(question_id)
        last_question_order = int(question["question_order_number"] or 0)
        _upsert_arena_progress(db, match, last_question_order)
        finished = match["my_lives"] == 0 or match["opponent_lives"] == 0
        save_arena_match(match_id, match)

        return ok(
            {
                "match_id": match_id,
                "question_id": question_id,
                "correct": player_correct,
                "timed_out": timed_out,
                "earned_score": earned_score,
                "total_score": match["my_score"],
                "round_result": round_result,
                "opponent_correct": question["opponent_correct"],
                "opponent_time_sec": question["opponent_time_sec"],
                "my_lives": match["my_lives"],
                "opponent_lives": match["opponent_lives"],
                "finished": finished,
                "last_question_order": last_question_order,
            }
        )
    except ValueError:
        return fail("BAD_REQUEST", "time_ms must be an integer", 400)
    except Exception:
        return _internal_error_response("submit_battle_answer")


@bp.post("/finish")
@require_user
def finish_battle():
    try:
        db = get_db()
        data = request.get_json(silent=True) or {}
        match_id = data.get("match_id")

        if not match_id:
            return fail("BAD_REQUEST", "match_id is required", 400)

        match, error_response = _load_match_for_user(match_id, g.get("user_id"))
        if error_response:
            return error_response

        if match["my_lives"] > match["opponent_lives"]:
            result = "win"
        elif match["my_lives"] < match["opponent_lives"]:
            result = "lose"
        elif match["my_score"] > match["opponent_score"]:
            result = "win"
        elif match["my_score"] < match["opponent_score"]:
            result = "lose"
        else:
            result = "draw"

        reward = _reward_for_result(result)
        rating_result = _update_arena_profile(db, match["user_id"], result, match["cp_gap"])
        if not rating_result:
            return fail("NOT_FOUND", "profile not found", 404)

        _delete_arena_progress(db, match["user_id"], match["opponent_id"], match["category"])

        response = {
            "match_id": match_id,
            "result": result,
            "my_score": match["my_score"],
            "opponent_score": match["opponent_score"],
            "my_lives": match["my_lives"],
            "opponent_lives": match["opponent_lives"],
            "reward": reward,
            **rating_result,
        }

        pop_arena_match(match_id)
        return ok(response)
    except Exception:
        return _internal_error_response("finish_battle")





