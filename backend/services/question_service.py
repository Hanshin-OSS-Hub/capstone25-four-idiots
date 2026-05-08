import random
import re

from sqlalchemy import text

PROFILE_CP_COLUMNS = {
    "concept": "cp_concept",
    "calc": "cp_calc",
    "idea": "cp_idea",
    "design": "cp_design",
    "practice": "cp_practical",
}
CATEGORY_DISPLAY_NAMES = {
    "concept": "\uac1c\ub150\uc774\ud574",
    "calc": "\uc5f0\uc0b0",
    "idea": "\ubc1c\uc0c1",
    "design": "\uc124\uacc4",
    "practice": "\uc2e4\uc804",
}
CATEGORY_TABLES = {
    "concept": {"table": "Q_CONCEPT", "answer_type": "choice"},
    "calc": {"table": "Q_CALC", "answer_type": "ocr"},
    "idea": {"table": "Q_IDEA", "answer_type": "choice"},
    "design": {"table": "Q_DESIGN", "answer_type": "order"},
    "practice": {"table": "Q_PRACTICAL", "answer_type": "ocr"},
}


def normalize_answer_value(value, answer_type):
    if isinstance(value, list):
        value = "-".join(str(item).strip() for item in value)

    normalized = "" if value is None else str(value).strip()
    normalized = normalized.replace("\u00a0", " ")
    normalized = normalized.replace("\u2212", "-").replace("\u2013", "-").replace("\u2014", "-")

    if answer_type == "order":
        normalized = normalized.replace(" ", "")
        normalized = normalized.replace(",", "-")
        normalized = re.sub(r"-+", "-", normalized)
        return normalized

    if answer_type == "ocr":
        normalized = normalized.replace(" ", "")
        normalized = normalized.replace("$", "")
        normalized = normalized.replace("\\", "")
        normalized = normalized.replace("₩", "")
        normalized = normalized.replace(",", "")
        return normalized.lower()

    return " ".join(normalized.split())


def normalize_category(raw_category):
    if not raw_category:
        return None

    normalized = str(raw_category).strip().lower()
    compact = normalized.replace(" ", "").replace("-", "").replace("_", "")
    aliases = {
        "concept": "concept",
        "calc": "calc",
        "calculation": "calc",
        "idea": "idea",
        "design": "design",
        "practice": "practice",
        "practical": "practice",
        "conceptunderstanding": "concept",
        "gaenyeom": "concept",
        "gaenyeomihae": "concept",
        "\uac1c\ub150\uc774\ud574": "concept",
        "\uac1c\ub150": "concept",
        "yeonsan": "calc",
        "\uc5f0\uc0b0": "calc",
        "balsang": "idea",
        "\ubc1c\uc0c1": "idea",
        "seolgye": "design",
        "\uc124\uacc4": "design",
        "siljeon": "practice",
        "\uc2e4\uc804": "practice",
    }
    return aliases.get(normalized) or aliases.get(compact)


def ensure_user_solved_question_table(db):
    db.execute(
        text(
            """
            CREATE TABLE IF NOT EXISTS USER_SOLVED_QUESTION (
                user_id VARCHAR(50) NOT NULL COMMENT '??? ???',
                category_name VARCHAR(50) NOT NULL COMMENT '???',
                q_id VARCHAR(50) NOT NULL COMMENT '?? ???',
                solved_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '??(??) ??',
                PRIMARY KEY (user_id, category_name, q_id),
                FOREIGN KEY (user_id) REFERENCES USER(user_id) ON DELETE CASCADE,
                INDEX idx_user_category (user_id, category_name)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
            """
        )
    )
    db.commit()


def _question_ids_for_difficulty(db, category, question_ids, difficulty):
    if not question_ids or not difficulty:
        return set()

    table_name = CATEGORY_TABLES[category]["table"]
    params = {"difficulty": difficulty}
    placeholders = []
    for index, question_id in enumerate(question_ids):
        key = f"question_id_{index}"
        placeholders.append(f":{key}")
        params[key] = question_id

    query = text(
        f"""
        SELECT q_id
        FROM {table_name}
        WHERE diff_name = :difficulty
          AND q_id IN ({', '.join(placeholders)})
        """
    )
    rows = db.execute(query, params).mappings().all()
    return {row["q_id"] for row in rows}


def get_user_history(db, user_id, category, difficulty=None):
    ensure_user_solved_question_table(db)
    category_name = CATEGORY_DISPLAY_NAMES[category]
    table_name = CATEGORY_TABLES[category]["table"]
    params = {"user_id": user_id, "category_name": category_name}
    difficulty_filter = ""
    if difficulty:
        params["difficulty"] = difficulty
        difficulty_filter = " AND q.diff_name = :difficulty"

    rows = db.execute(
        text(
            f"""
            SELECT usq.q_id
            FROM USER_SOLVED_QUESTION usq
            JOIN {table_name} q ON q.q_id = usq.q_id
            WHERE usq.user_id = :user_id
              AND usq.category_name = :category_name
              {difficulty_filter}
            """
        ),
        params,
    ).mappings().all()
    return {row["q_id"] for row in rows}


def remember_question(db, user_id, category, question_id):
    ensure_user_solved_question_table(db)
    category_name = CATEGORY_DISPLAY_NAMES[category]
    db.execute(
        text(
            """
            INSERT INTO USER_SOLVED_QUESTION (user_id, category_name, q_id)
            VALUES (:user_id, :category_name, :q_id)
            ON DUPLICATE KEY UPDATE solved_at = CURRENT_TIMESTAMP
            """
        ),
        {
            "user_id": user_id,
            "category_name": category_name,
            "q_id": question_id,
        },
    )
    db.commit()


def reset_user_history(db, user_id, category, difficulty=None):
    ensure_user_solved_question_table(db)
    category_name = CATEGORY_DISPLAY_NAMES[category]
    table_name = CATEGORY_TABLES[category]["table"]
    params = {"user_id": user_id, "category_name": category_name}
    if difficulty:
        params["difficulty"] = difficulty
        db.execute(
            text(
                f"""
                DELETE usq
                FROM USER_SOLVED_QUESTION usq
                JOIN {table_name} q ON q.q_id = usq.q_id
                WHERE usq.user_id = :user_id
                  AND usq.category_name = :category_name
                  AND q.diff_name = :difficulty
                """
            ),
            params,
        )
    else:
        db.execute(
            text(
                """
                DELETE FROM USER_SOLVED_QUESTION
                WHERE user_id = :user_id AND category_name = :category_name
                """
            ),
            params,
        )
    db.commit()


def build_question_payload(category, row):
    answer_type = CATEGORY_TABLES[category]["answer_type"]
    payload = {
        "question_id": row["q_id"],
        "category": category,
        "difficulty": row["diff_name"],
        "text": row["content"],
        "answer_type": answer_type,
        "choices": [],
    }
    if "score" in row:
        payload["score"] = row["score"]
    if "difficulty_icon_url" in row:
        payload["difficulty_icon_url"] = row.get("difficulty_icon_url")

    if answer_type in {"choice", "order"}:
        payload["choices"] = [row["opt1"], row["opt2"], row["opt3"], row["opt4"]]
    return payload


def parse_order_answer(order_answer):
    tokens = str(order_answer or "").replace(" ", "").split("-")
    parsed = []
    for token in tokens:
        if token.isdigit():
            parsed.append(int(token) - 1)
    return parsed


def prepare_question_for_delivery(category, row):
    payload = build_question_payload(category, row)
    answer_type = CATEGORY_TABLES[category]["answer_type"]
    correct_answer = str(row.get("correct_answer", "")).strip()

    if answer_type not in {"choice", "order"}:
        return payload, correct_answer

    original_choices = [row["opt1"], row["opt2"], row["opt3"], row["opt4"]]
    shuffle_order = list(range(len(original_choices)))
    random.shuffle(shuffle_order)
    payload["choices"] = [original_choices[index] for index in shuffle_order]

    if answer_type == "order":
        target_order = parse_order_answer(correct_answer)
        displayed_position_by_original = {
            original_index: displayed_index + 1
            for displayed_index, original_index in enumerate(shuffle_order)
        }
        correct_answer = "-".join(
            str(displayed_position_by_original[index])
            for index in target_order
            if index in displayed_position_by_original
        )

    return payload, correct_answer


def get_answer_column(category):
    if category in {"calc", "practice"}:
        return "ocr_answer"
    if category == "design":
        return "order_answer"
    return "answer"


def _question_select_columns(category, answer_column):
    common = [
        "q.q_id",
        "q.diff_name",
        "q.content",
        f"q.{answer_column} AS correct_answer",
        "d.score",
        "d.icon_url AS difficulty_icon_url",
    ]
    if CATEGORY_TABLES[category]["answer_type"] in {"choice", "order"}:
        common[3:3] = ["q.opt1", "q.opt2", "q.opt3", "q.opt4"]
    return ",\n               ".join(common)


def load_question(db, category, question_id):
    table_name = CATEGORY_TABLES[category]["table"]
    answer_column = get_answer_column(category)
    select_columns = _question_select_columns(category, answer_column)
    query = text(
        f"""
        SELECT {select_columns}
        FROM {table_name} q
        JOIN DIFFICULTY d ON d.diff_name = q.diff_name
        WHERE q.q_id = :question_id
        """
    )
    return db.execute(query, {"question_id": question_id}).mappings().first()


def load_random_question(db, category, difficulty=None, excluded_ids=None):
    table_name = CATEGORY_TABLES[category]["table"]
    answer_column = get_answer_column(category)
    select_columns = _question_select_columns(category, answer_column)
    excluded_ids = excluded_ids or []

    base_query = f"""
    SELECT {select_columns}
    FROM {table_name} q
    JOIN DIFFICULTY d ON d.diff_name = q.diff_name
    WHERE 1=1
    """

    params = {}
    if difficulty:
        base_query += " AND q.diff_name = :difficulty"
        params["difficulty"] = difficulty

    if excluded_ids:
        placeholders = []
        for index, question_id in enumerate(excluded_ids):
            key = f"excluded_{index}"
            placeholders.append(f":{key}")
            params[key] = question_id
        base_query += f" AND q.q_id NOT IN ({', '.join(placeholders)})"

    base_query += " ORDER BY RAND() LIMIT 1"
    return db.execute(text(base_query), params).mappings().first()


def load_question_detail(db, category, question_id):
    table_name = CATEGORY_TABLES[category]["table"]
    answer_column = get_answer_column(category)
    select_columns = _question_select_columns(category, answer_column)
    query = text(
        f"""
        SELECT {select_columns}
        FROM {table_name} q
        JOIN DIFFICULTY d ON d.diff_name = q.diff_name
        WHERE q.q_id = :question_id
        """
    )
    return db.execute(query, {"question_id": question_id}).mappings().first()
