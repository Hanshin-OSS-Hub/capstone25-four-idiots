from sqlalchemy import text

from services.question_service import CATEGORY_DISPLAY_NAMES, CATEGORY_TABLES, get_answer_column
from services.tier_service import normalize_tier_name, tier_name_to_api

MAX_ARENA_TIME_MS = 60000
CATEGORY_CP_COLUMNS = {
    "concept": "cp_concept",
    "calc": "cp_calc",
    "idea": "cp_idea",
    "design": "cp_design",
    "practice": "cp_practical",
}
TIER_STAGES = ["normal", "core", "magic", "rare", "elite", "unique", "legend"]
BASE_TIERS = ["bronze", "silver", "gold", "platinum", "diamond", "master", "challenger"]
MAX_TIER_LEVEL = len(TIER_STAGES) * len(BASE_TIERS) - 1


def load_opponent_set(db, set_id, category):
    category_name = CATEGORY_DISPLAY_NAMES[category]
    records_query = text(
        """
        SELECT q_id, question_order_number, solve_time_sec, is_correct
        FROM TRAINING_Q_SET_RECORD
        WHERE set_id = :set_id AND category_name = :category_name
        ORDER BY question_order_number ASC
        """
    )
    return db.execute(records_query, {"set_id": set_id, "category_name": category_name}).mappings().all()



def score_answer(question, submitted_answer, time_ms):
    timed_out = time_ms >= MAX_ARENA_TIME_MS
    correct_answer = str(question["correct_answer"]).strip()
    if isinstance(submitted_answer, list):
        user_answer = "-".join(str(item).strip() for item in submitted_answer)
    else:
        user_answer = "" if submitted_answer is None else str(submitted_answer).strip()
    is_correct = (not timed_out) and user_answer == correct_answer
    earned_score = int(question["score"] or 0) if is_correct else 0
    return is_correct, timed_out, earned_score



def resolve_round(player_correct, player_time_ms, opponent_correct, opponent_time_sec):
    opponent_time_ms = int(opponent_time_sec or 0) * 1000
    if player_correct and not opponent_correct:
        return "player_attack"
    if opponent_correct and not player_correct:
        return "opponent_attack"
    if player_correct and opponent_correct:
        if player_time_ms < opponent_time_ms:
            return "player_attack"
        if player_time_ms > opponent_time_ms:
            return "opponent_attack"
    return "draw"



def reward_for_result(result):
    if result == "win":
        return {"gold": 30, "xp": 20}
    if result == "draw":
        return {"gold": 15, "xp": 10}
    return {"gold": 10, "xp": 5}



def arena_rating_delta(cp_gap, result):
    if result == "draw":
        return 0
    if cp_gap < 50:
        return 5 if result == "win" else -15
    if cp_gap <= 100:
        return 10 if result == "win" else -10
    return 15 if result == "win" else -5



def tier_name_from_level(level):
    stage_index = level % len(TIER_STAGES)
    base_index = level // len(TIER_STAGES)
    return f"{TIER_STAGES[stage_index]} {BASE_TIERS[base_index]}"



def apply_arena_rating(current_tier_name, current_rating, delta):
    normalized_tier_name = normalize_tier_name(current_tier_name)
    try:
        stage_name, base_tier = str(normalized_tier_name).split(" ", 1)
        current_level = BASE_TIERS.index(base_tier) * len(TIER_STAGES) + TIER_STAGES.index(stage_name)
    except ValueError:
        current_level = 0

    total_points = current_level * 100 + int(current_rating or 0) + int(delta or 0)
    max_total_points = MAX_TIER_LEVEL * 100 + 99
    total_points = max(0, min(total_points, max_total_points))

    new_level = total_points // 100
    new_rating = total_points % 100
    return tier_name_from_level(new_level), new_rating



def load_my_profile(db, user_id, cp_column):
    query = text(
        f"""
        SELECT u.user_id, u.nickname, p.tier_name, p.arena_rating, p.{cp_column} AS my_power
        FROM USER u
        JOIN PROFILE p ON p.user_id = u.user_id
        WHERE u.user_id = :user_id
        """
    )
    return db.execute(query, {"user_id": user_id}).mappings().first()



def update_arena_profile(db, user_id, result, cp_gap):
    profile_query = text(
        """
        SELECT tier_name, arena_rating
        FROM PROFILE
        WHERE user_id = :user_id
        """
    )
    profile = db.execute(profile_query, {"user_id": user_id}).mappings().first()
    if not profile:
        return None

    rating_delta = arena_rating_delta(cp_gap, result)
    new_tier, new_rating = apply_arena_rating(profile["tier_name"], profile["arena_rating"], rating_delta)
    update_query = text(
        """
        UPDATE PROFILE
        SET tier_name = :tier_name,
            arena_rating = :arena_rating
        WHERE user_id = :user_id
        """
    )
    db.execute(update_query, {"tier_name": new_tier, "arena_rating": new_rating, "user_id": user_id})
    db.commit()
    return {
        "rating_delta": rating_delta,
        "previous_tier": tier_name_to_api(profile["tier_name"]),
        "previous_arena_rating": int(profile["arena_rating"] or 0),
        "current_tier": tier_name_to_api(new_tier),
        "current_arena_rating": int(new_rating),
    }



def load_arena_progress(db, user_id, opponent_id, category):
    category_name = CATEGORY_DISPLAY_NAMES[category]
    query = text(
        """
        SELECT user_id, opponent_id, category_name, updated_cp, set_id, last_question_order
        FROM ARENA_PROGRESS
        WHERE user_id = :user_id
          AND opponent_id = :opponent_id
          AND category_name = :category_name
        """
    )
    return db.execute(query, {"user_id": user_id, "opponent_id": opponent_id, "category_name": category_name}).mappings().first()



def upsert_arena_progress(db, match, last_question_order):
    category_name = CATEGORY_DISPLAY_NAMES[match["category"]]
    existing = load_arena_progress(db, match["user_id"], match["opponent_id"], match["category"])
    params = {
        "user_id": match["user_id"],
        "opponent_id": match["opponent_id"],
        "category_name": category_name,
        "updated_cp": match["updated_cp"],
        "set_id": match["set_id"],
        "last_question_order": last_question_order,
    }
    if existing:
        query = text(
            """
            UPDATE ARENA_PROGRESS
            SET updated_cp = :updated_cp,
                set_id = :set_id,
                last_question_order = :last_question_order
            WHERE user_id = :user_id
              AND opponent_id = :opponent_id
              AND category_name = :category_name
            """
        )
    else:
        query = text(
            """
            INSERT INTO ARENA_PROGRESS (
                user_id, opponent_id, category_name, updated_cp, set_id, last_question_order
            )
            VALUES (
                :user_id, :opponent_id, :category_name, :updated_cp, :set_id, :last_question_order
            )
            """
        )
    db.execute(query, params)
    db.commit()



def delete_arena_progress(db, user_id, opponent_id, category):
    category_name = CATEGORY_DISPLAY_NAMES[category]
    query = text(
        """
        DELETE FROM ARENA_PROGRESS
        WHERE user_id = :user_id
          AND opponent_id = :opponent_id
          AND category_name = :category_name
        """
    )
    db.execute(query, {"user_id": user_id, "opponent_id": opponent_id, "category_name": category_name})
    db.commit()



def rotate_records(records, last_question_order):
    if not records:
        return records
    start_index = last_question_order % len(records)
    return records[start_index:] + records[:start_index]
