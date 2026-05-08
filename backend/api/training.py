import logging
from uuid import uuid4

from flask import Blueprint, request, g
from sqlalchemy import text

from common.responses import fail, ok
from database import get_db
from .auth import require_user
from services.runtime_state import create_training_session, get_training_session, pop_training_session, save_training_session

bp = Blueprint("training", __name__, url_prefix="/v1/training")
logger = logging.getLogger(__name__)

MAX_LIVES = 4
MAX_TIME_SEC = 60
from services.question_service import (
    CATEGORY_TABLES,
    CATEGORY_DISPLAY_NAMES,
    _question_ids_for_difficulty as _question_ids_for_difficulty,
    get_user_history as _get_user_history,
    load_question as _load_question,
    load_random_question as _load_random_question,
    normalize_category as _normalize_category,
    normalize_answer_value as _normalize_answer_value,
    prepare_question_for_delivery as _prepare_question_for_delivery,
    remember_question as _remember_question,
    reset_user_history as _reset_user_history,
)
from services.training_service import (
    PROFILE_CP_COLUMNS,
    apply_training_result as _apply_training_result,
    load_profile_stats as _load_profile_stats,
    pick_difficulty as _pick_difficulty,
    save_training_records as _save_training_records,
)


def _create_training_session(user_id, category):
    session_id = f"train-{uuid4().hex[:10]}"
    db = get_db()
    profile = _load_profile_stats(db, user_id)
    starting_power = 0
    if profile:
        starting_power = int(profile[PROFILE_CP_COLUMNS[category]] or 0)
    create_training_session(
        session_id,
        {
            "user_id": user_id,
            "category": category,
            "starting_power": starting_power,
            "lives": MAX_LIVES,
            "total_power": 0,
            "question_count": 0,
            "asked_ids": [],
            "served_answers": {},
            "records": [],
        },
    )
    return session_id


def _get_training_session(session_id):
    return get_training_session(session_id)


def _internal_error_response(action):
    logger.exception("Training API error during %s", action)
    return fail("INTERNAL_ERROR", "Unexpected server error", 500)


@bp.post("/start")
@require_user
def start_training():
    data = request.get_json(silent=True) or {}
    category = _normalize_category(data.get("category") or data.get("categoryName"))

    if not category:
        return fail("BAD_REQUEST", "category is required", 400)

    session_id = _create_training_session(g.get("user_id"), category)
    session = get_training_session(session_id)
    return ok(
        {
            "session_id": session_id,
            "category": category,
            "category_name": CATEGORY_DISPLAY_NAMES[category],
            "starting_power": session.get("starting_power", 0),
            "lives": session["lives"],
            "remaining_lives": session["lives"],
            "total_power": session["total_power"],
            "question_count": session["question_count"],
            "max_time_sec": MAX_TIME_SEC,
        }
    )


@bp.get("/question")
@require_user
def get_random_question():
    try:
        db = get_db()
        category = _normalize_category(request.args.get("category") or request.args.get("categoryName"))
        difficulty = request.args.get("difficulty")
        session_id = request.args.get("session_id")
        user_id = g.get("user_id")

        if session_id:
            session = _get_training_session(session_id)
            if not session:
                return fail("NOT_FOUND", "training session not found", 404)
            if session["lives"] <= 0:
                return fail("FINISHED", "training session already finished", 409)
            category = session["category"]
            difficulty = _pick_difficulty(
                session["question_count"],
                int(session.get("starting_power", 0) or 0),
            )
            excluded_ids = _question_ids_for_difficulty(db, category, session["asked_ids"], difficulty)
            excluded_ids.update(_question_ids_for_difficulty(db, category, session["served_answers"].keys(), difficulty))
        else:
            excluded_ids = set()

        if not category:
            return fail("BAD_REQUEST", "category is required", 400)

        excluded_ids.update(_get_user_history(db, user_id, category, difficulty=difficulty))
        row = _load_random_question(db, category, difficulty=difficulty, excluded_ids=list(excluded_ids))

        recycled = False
        if not row and excluded_ids:
            _reset_user_history(db, user_id, category, difficulty=difficulty)
            if session_id:
                session["asked_ids"] = [qid for qid in session["asked_ids"] if qid not in excluded_ids]
                session["served_answers"] = {
                    qid: answer
                    for qid, answer in session["served_answers"].items()
                    if qid not in excluded_ids
                }
                save_training_session(session_id, session)
            row = _load_random_question(db, category, difficulty=difficulty, excluded_ids=[])
            recycled = True
        if not row:
            return fail("NO_QUESTIONS", "no question found", 404)

        payload, correct_answer = _prepare_question_for_delivery(category, row)
        payload["recycled"] = recycled
        if session_id:
            session["served_answers"][row["q_id"]] = correct_answer
            save_training_session(session_id, session)
            payload["session_id"] = session_id
            payload["remaining_lives"] = session["lives"]
            payload["total_power"] = session["total_power"]
            payload["question_order"] = session["question_count"] + 1
        return ok(payload)
    except Exception:
        return _internal_error_response("get_question")


@bp.post("/submit")
@require_user
def submit_result():
    try:
        db = get_db()
        data = request.get_json(silent=True) or {}

        category = _normalize_category(data.get("category") or data.get("categoryName"))
        question_id = data.get("question_id")
        submitted_answer = data.get("answer")
        if submitted_answer is None:
            submitted_answer = data.get("choice")
        if submitted_answer is None:
            submitted_answer = data.get("answer_order", data.get("answerOrder"))
        time_sec = int(data.get("time_sec", 0))
        session_id = data.get("session_id")
        user_id = g.get("user_id")

        if not session_id or not question_id:
            return fail("BAD_REQUEST", "session_id and question_id are required", 400)

        session = _get_training_session(session_id)
        if not session:
            return fail("NOT_FOUND", "training session not found", 404)
        if session["lives"] <= 0:
            return fail("FINISHED", "training session already finished", 409)
        category = session["category"]
        if question_id in session["asked_ids"]:
            return fail("BAD_REQUEST", "question already submitted", 400)
        if question_id not in session["served_answers"]:
            return fail("BAD_REQUEST", "question was not served for this session", 400)

        row = _load_question(db, category, question_id)
        if not row:
            return fail("NOT_FOUND", "question not found", 404)

        timed_out = time_sec >= MAX_TIME_SEC
        answer_type = CATEGORY_TABLES[category]["answer_type"]
        response_correct_answer = row["correct_answer"]
        response_correct_answer = session["served_answers"].get(question_id, response_correct_answer)
        correct_answer = _normalize_answer_value(response_correct_answer, answer_type)
        user_answer = _normalize_answer_value(submitted_answer, answer_type)
        is_correct = (not timed_out) and user_answer == correct_answer
        earned_score = row["score"] if is_correct else 0

        _remember_question(db, user_id, category, question_id)

        response = {
            "question_id": question_id,
            "category": category,
            "correct": is_correct,
            "correct_answer": response_correct_answer,
            "earned_score": earned_score,
            "time_sec": time_sec,
            "timed_out": timed_out,
        }

        if question_id not in session["asked_ids"]:
            session["asked_ids"].append(question_id)
        session["served_answers"].pop(question_id, None)
        session["question_count"] += 1
        if is_correct:
            session["total_power"] += earned_score
        else:
            session["lives"] = max(0, session["lives"] - 1)

        session["records"].append(
            {
                "question_id": question_id,
                "time_sec": min(time_sec, MAX_TIME_SEC),
                "is_correct": is_correct,
            }
        )

        save_training_session(session_id, session)

        response.update(
            {
                "session_id": session_id,
                "lives": session["lives"],
                "remaining_lives": session["lives"],
                "total_power": session["total_power"],
                "question_count": session["question_count"],
                "finished": session["lives"] == 0,
            }
        )

        return ok(response)
    except ValueError:
        return fail("BAD_REQUEST", "time_sec must be an integer", 400)
    except Exception:
        return _internal_error_response("submit_result")


@bp.post("/finish")
@require_user
def finish_training():
    try:
        db = get_db()
        data = request.get_json(silent=True) or {}
        session_id = data.get("session_id")

        if not session_id:
            return fail("BAD_REQUEST", "session_id is required", 400)

        session = _get_training_session(session_id)
        if not session:
            return fail("NOT_FOUND", "training session not found", 404)

        profile_result = _apply_training_result(
            db,
            user_id=session["user_id"],
            category=session["category"],
            total_power=session["total_power"],
        )
        if not profile_result:
            return fail("NOT_FOUND", "profile not found", 404)

        set_id = None
        if profile_result["updated"]:
            set_id = _save_training_records(db, session, profile_result["nickname"])

        result = {
            "session_id": session_id,
            "category": session["category"],
            "total_power": session["total_power"],
            "lives": session["lives"],
            "remaining_lives": session["lives"],
            "question_count": session["question_count"],
            "set_id": set_id,
            **profile_result,
        }

        pop_training_session(session_id)
        return ok(result)
    except Exception:
        return _internal_error_response("finish_training")



