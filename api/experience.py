import logging
from uuid import uuid4

from flask import Blueprint, g, request

from common.responses import fail, ok
from database import get_db
from .auth import require_user
from services.runtime_state import create_experience_session, get_experience_session, pop_experience_session, save_experience_session
from services.question_service import (
    CATEGORY_DISPLAY_NAMES,
    get_user_history as _get_user_history,
    load_question as _load_question,
    load_random_question as _load_random_question,
    normalize_category as _normalize_category,
    prepare_question_for_delivery as _prepare_question_for_delivery,
    remember_question as _remember_question,
    reset_user_history as _reset_user_history,
)
from services.training_service import PROFILE_CP_COLUMNS, load_profile_stats as _load_profile_stats

bp = Blueprint("experience", __name__, url_prefix="/v1/experience")
logger = logging.getLogger(__name__)



def _internal_error_response(action):
    logger.exception("Experience API error during %s", action)
    return fail("INTERNAL_ERROR", "Unexpected server error", 500)


def _get_experience_session(session_id):
    return get_experience_session(session_id)


def _experience_max_questions(current_power):
    if current_power >= 500:
        return 50
    if current_power >= 200:
        return 40
    return 30


def _experience_difficulty(question_count, current_power):
    order = question_count + 1
    if current_power >= 500:
        if order <= 5:
            return "VERY EASY"
        if order <= 10:
            return "EASY"
        if order <= 15:
            return "HARD"
        if order <= 20:
            return "VERY HARD"
        if order <= 30:
            return "TOUGH"
        return "VERY TOUGH"
    if current_power >= 200:
        if order <= 10:
            return "VERY EASY"
        if order <= 20:
            return "EASY"
        if order <= 30:
            return "HARD"
        return "VERY HARD"
    if order <= 20:
        return "VERY EASY"
    return "EASY"


@bp.post("/start")
@require_user
def start_experience():
    data = request.get_json(silent=True) or {}
    category = _normalize_category(data.get("category") or data.get("categoryName"))
    if not category:
        return fail("BAD_REQUEST", "category is required", 400)

    db = get_db()
    profile = _load_profile_stats(db, g.get("user_id"))
    if not profile:
        return fail("NOT_FOUND", "profile not found", 404)

    current_power = int(profile[PROFILE_CP_COLUMNS[category]] or 0)
    session_id = f"exp-{uuid4().hex[:10]}"
    max_questions = _experience_max_questions(current_power)
    create_experience_session(session_id, {
        "user_id": g.get("user_id"),
        "category": category,
        "question_count": 0,
        "correct_count": 0,
        "asked_ids": [],
        "served_answers": {},
        "current_power": current_power,
        "max_questions": max_questions,
    })

    return ok(
        {
            "session_id": session_id,
            "category": category,
            "category_name": CATEGORY_DISPLAY_NAMES[category],
            "question_count": 0,
            "correct_count": 0,
            "current_power": current_power,
            "max_questions": max_questions,
            "remaining_questions": max_questions,
        }
    )


@bp.get("/question")
@require_user
def get_experience_question():
    try:
        db = get_db()
        session_id = request.args.get("session_id")
        if not session_id:
            return fail("BAD_REQUEST", "session_id is required", 400)

        session = _get_experience_session(session_id)
        if not session:
            return fail("NOT_FOUND", "experience session not found", 404)

        if session["question_count"] >= session["max_questions"]:
            return fail("FINISHED", "experience session already finished", 409)

        category = session["category"]
        difficulty = _experience_difficulty(session["question_count"], session["current_power"])
        excluded_ids = set(session["asked_ids"])
        excluded_ids.update(_get_user_history(db, session["user_id"], category))
        row = _load_random_question(db, category, difficulty=difficulty, excluded_ids=list(excluded_ids))

        recycled = False
        if not row and excluded_ids:
            _reset_user_history(db, session["user_id"], category)
            session["asked_ids"] = []
            save_experience_session(session_id, session)
            row = _load_random_question(db, category, difficulty=difficulty, excluded_ids=[])
            recycled = True

        if not row:
            return fail("NO_QUESTIONS", "no question found", 404)

        payload, correct_answer = _prepare_question_for_delivery(category, row)
        session["served_answers"][row["q_id"]] = correct_answer
        save_experience_session(session_id, session)
        payload.update(
            {
                "session_id": session_id,
                "category_name": CATEGORY_DISPLAY_NAMES[category],
                "recycled": recycled,
                "question_order": session["question_count"] + 1,
                "correct_count": session["correct_count"],
                "max_questions": session["max_questions"],
                "remaining_questions": session["max_questions"] - session["question_count"],
            }
        )
        return ok(payload)
    except Exception:
        return _internal_error_response("get_experience_question")


@bp.post("/submit")
@require_user
def submit_experience_answer():
    try:
        db = get_db()
        data = request.get_json(silent=True) or {}
        session_id = data.get("session_id") or data.get("sessionId")
        question_id = data.get("question_id") or data.get("questionId") or data.get("q_id") or data.get("qId")
        submitted_answer = data.get("answer")
        if submitted_answer is None:
            submitted_answer = data.get("choice")
        if submitted_answer is None:
            submitted_answer = data.get("answer_order", data.get("answerOrder"))

        if not session_id or not question_id:
            return fail("BAD_REQUEST", "session_id and question_id are required", 400)

        session = _get_experience_session(session_id)
        if not session:
            return fail("NOT_FOUND", "experience session not found", 404)

        if session["question_count"] >= session["max_questions"]:
            return fail("FINISHED", "experience session already finished", 409)

        category = session["category"]
        row = _load_question(db, category, question_id)
        if not row:
            return fail("NOT_FOUND", "question not found", 404)

        correct_answer = str(row["correct_answer"]).strip()
        correct_answer = session["served_answers"].get(question_id, correct_answer)
        if isinstance(submitted_answer, list):
            user_answer = "-".join(str(item).strip() for item in submitted_answer)
        else:
            user_answer = "" if submitted_answer is None else str(submitted_answer).strip()
        is_correct = user_answer == correct_answer
        earned_score = int(row["score"] or 0) if is_correct else 0

        if question_id not in session["asked_ids"]:
            session["asked_ids"].append(question_id)
        _remember_question(db, session["user_id"], category, question_id)
        session["question_count"] += 1
        if is_correct:
            session["correct_count"] += 1

        finished = session["question_count"] >= session["max_questions"]
        save_experience_session(session_id, session)

        return ok(
            {
                "session_id": session_id,
                "category": category,
                "category_name": CATEGORY_DISPLAY_NAMES[category],
                "question_id": question_id,
                "correct": is_correct,
                "earned_score": earned_score,
                "question_count": session["question_count"],
                "correct_count": session["correct_count"],
                "max_questions": session["max_questions"],
                "remaining_questions": max(0, session["max_questions"] - session["question_count"]),
                "finished": finished,
            }
        )
    except Exception:
        return _internal_error_response("submit_experience_answer")


@bp.post("/finish")
@require_user
def finish_experience():
    data = request.get_json(silent=True) or {}
    session_id = data.get("session_id") or data.get("sessionId")
    if not session_id:
        return fail("BAD_REQUEST", "session_id is required", 400)

    session = _get_experience_session(session_id)
    if not session:
        return fail("NOT_FOUND", "experience session not found", 404)

    result = {
        "session_id": session_id,
        "category": session["category"],
        "category_name": CATEGORY_DISPLAY_NAMES[session["category"]],
        "question_count": session["question_count"],
        "correct_count": session["correct_count"],
        "max_questions": session["max_questions"],
        "remaining_questions": max(0, session["max_questions"] - session["question_count"]),
        "current_power": session["current_power"],
    }
    pop_experience_session(session_id)
    return ok(result)


