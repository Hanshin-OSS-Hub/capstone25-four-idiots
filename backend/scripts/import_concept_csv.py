import csv
import os
import sys
from pathlib import Path

import pymysql
from dotenv import load_dotenv

load_dotenv()

DB_HOST = os.getenv("MYSQL_HOST", "127.0.0.1")
DB_USER = os.getenv("MYSQL_USER", "root")
DB_PASSWORD = os.getenv("MYSQL_PASSWORD", "")
DB_NAME = os.getenv("MYSQL_DATABASE", "math_arena")
DB_PORT = int(os.getenv("MYSQL_PORT", "3306"))

CSV_PATH = r"D:\capstone1_server\data\csv\1. 개념이해 문제.csv"

COLUMN_MAP = {
    "??ID": "q_id",
    "????": "category_name",
    "???": "diff_name",
    "??": "content",
    "??1": "opt1",
    "??2": "opt2",
    "??3": "opt3",
    "??4": "opt4",
    "???": "answer",
}

REQUIRED_COLUMNS = [
    "q_id",
    "category_name",
    "diff_name",
    "content",
    "opt1",
    "opt2",
    "opt3",
    "opt4",
    "answer",
]

UPSERT_SQL = """
INSERT INTO Q_CONCEPT (
    q_id, category_name, diff_name, content, opt1, opt2, opt3, opt4, answer
) VALUES (
    %s, %s, %s, %s, %s, %s, %s, %s, %s
)
ON DUPLICATE KEY UPDATE
    category_name = VALUES(category_name),
    diff_name = VALUES(diff_name),
    content = VALUES(content),
    opt1 = VALUES(opt1),
    opt2 = VALUES(opt2),
    opt3 = VALUES(opt3),
    opt4 = VALUES(opt4),
    answer = VALUES(answer)
"""


def get_connection():
    return pymysql.connect(
        host=DB_HOST,
        user=DB_USER,
        password=DB_PASSWORD,
        db=DB_NAME,
        port=DB_PORT,
        charset="utf8mb4",
        cursorclass=pymysql.cursors.DictCursor,
    )



def normalize_row(raw_row, line_no):
    row = {}
    for src, dest in COLUMN_MAP.items():
        value = raw_row.get(src, "")
        row[dest] = "" if value is None else str(value).strip()

    missing = [col for col in REQUIRED_COLUMNS if not row.get(col)]
    if missing:
        raise ValueError(f"line {line_no}: missing required values for {missing}")

    return (
        row["q_id"],
        row["category_name"],
        row["diff_name"],
        row["content"],
        row["opt1"],
        row["opt2"],
        row["opt3"],
        row["opt4"],
        row["answer"],
    )



def main():
    csv_file = Path(CSV_PATH)
    if not csv_file.exists():
        print(f"CSV not found: {csv_file}")
        sys.exit(1)

    with csv_file.open("r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)
        headers = reader.fieldnames or []
        missing_headers = [name for name in COLUMN_MAP if name not in headers]
        if missing_headers:
            print(f"Missing CSV headers: {missing_headers}")
            sys.exit(1)

        rows = []
        for index, raw_row in enumerate(reader, start=2):
            rows.append(normalize_row(raw_row, index))

    if not rows:
        print("No rows to import")
        return

    conn = get_connection()
    try:
        with conn.cursor() as cursor:
            cursor.executemany(UPSERT_SQL, rows)
        conn.commit()
        print(f"Imported {len(rows)} concept questions into Q_CONCEPT")
    finally:
        conn.close()


if __name__ == "__main__":
    main()
