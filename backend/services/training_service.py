from uuid import uuid4

from sqlalchemy import text

from services.question_service import CATEGORY_DISPLAY_NAMES, PROFILE_CP_COLUMNS


def difficulty_filter(total_power):
    if total_power >= 500:
        return [
            (5, "VERY EASY"),
            (10, "EASY"),
            (15, "HARD"),
            (20, "VERY HARD"),
            (30, "TOUGH"),
            (10**9, "VERY TOUGH"),
        ]
    if total_power >= 200:
        return [
            (10, "VERY EASY"),
            (20, "EASY"),
            (30, "HARD"),
            (10**9, "VERY HARD"),
        ]
    return [
        (20, "VERY EASY"),
        (10**9, "EASY"),
    ]


def pick_difficulty(question_count, total_power):
    order = question_count + 1
    for limit, difficulty in difficulty_filter(total_power):
        if order <= limit:
            return difficulty
    return None


def load_profile_stats(db, user_id):
    query = text(
        """
        SELECT user_id, nickname, cp_concept, cp_calc, cp_idea, cp_design, cp_practical
        FROM PROFILE
        WHERE user_id = :user_id
        """
    )
    return db.execute(query, {"user_id": user_id}).mappings().first()


def average_cp(profile_row):
    values = [
        int(profile_row["cp_concept"]),
        int(profile_row["cp_calc"]),
        int(profile_row["cp_idea"]),
        int(profile_row["cp_design"]),
        int(profile_row["cp_practical"]),
    ]
    return int(sum(values) / len(values))


def apply_training_result(db, user_id, category, total_power):
    profile = load_profile_stats(db, user_id)
    if not profile:
        return None

    cp_column = PROFILE_CP_COLUMNS[category]
    current_value = int(profile[cp_column])
    updated = total_power > current_value

    if updated:
        update_query = text(
            f"""
            UPDATE PROFILE
            SET {cp_column} = :new_value
            WHERE user_id = :user_id
            """
        )
        db.execute(update_query, {"new_value": total_power, "user_id": user_id})
        db.commit()
        profile = load_profile_stats(db, user_id)

    return {
        "updated": updated,
        "previous_power": current_value,
        "current_power": int(profile[cp_column]),
        "average_power": average_cp(profile),
        "nickname": profile["nickname"],
        "cp_concept": int(profile["cp_concept"]),
        "cp_calc": int(profile["cp_calc"]),
        "cp_idea": int(profile["cp_idea"]),
        "cp_design": int(profile["cp_design"]),
        "cp_practical": int(profile["cp_practical"]),
    }


def save_training_records(db, session, nickname):
    set_id = f"set-{uuid4().hex[:12]}"
    category_name = CATEGORY_DISPLAY_NAMES[session["category"]]

    match_query = text(
        """
        INSERT INTO RECORD_BATTLE_MATCH (set_id, user_id, nickname, category_name, updated_cp)
        VALUES (:set_id, :user_id, :nickname, :category_name, :updated_cp)
        """
    )
    db.execute(
        match_query,
        {
            "set_id": set_id,
            "user_id": session["user_id"],
            "nickname": nickname,
            "category_name": category_name,
            "updated_cp": session["total_power"],
        },
    )

    detail_query = text(
        """
        INSERT INTO TRAINING_Q_SET_RECORD (
            set_id,
            question_order_number,
            category_name,
            q_id,
            solve_time_sec,
            is_correct
        )
        VALUES (
            :set_id,
            :question_order_number,
            :category_name,
            :q_id,
            :solve_time_sec,
            :is_correct
        )
        """
    )

    for index, record in enumerate(session["records"], start=1):
        db.execute(
            detail_query,
            {
                "set_id": set_id,
                "question_order_number": index,
                "category_name": category_name,
                "q_id": record["question_id"],
                "solve_time_sec": record["time_sec"],
                "is_correct": record["is_correct"],
            },
        )

    db.commit()
    return set_id
