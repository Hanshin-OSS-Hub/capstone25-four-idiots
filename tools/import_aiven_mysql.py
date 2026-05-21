import argparse
import os
from pathlib import Path
from urllib.parse import urlparse

import pymysql


ROOT = Path(__file__).resolve().parents[1]
DEFAULT_SQL_FILES = [
    ROOT / "SQL" / "4. Math ARENA_MySQL.sql",
    ROOT / "data" / "concept_insert.sql",
    ROOT / "data" / "calc_insert.sql",
    ROOT / "data" / "idea_insert.sql",
    ROOT / "data" / "design_insert.sql",
    ROOT / "data" / "practical_insert.sql",
]


def parse_mysql_url(url):
    parsed = urlparse(url)
    if parsed.scheme not in {"mysql", "mysql+pymysql"}:
        raise ValueError("AIVEN_MYSQL_URL must start with mysql:// or mysql+pymysql://")
    return {
        "host": parsed.hostname,
        "port": parsed.port or 3306,
        "user": parsed.username,
        "password": parsed.password,
        "database": parsed.path.lstrip("/") or None,
    }


def split_sql_statements(sql_text):
    statements = []
    current = []
    in_single = False
    in_double = False
    escape = False

    for char in sql_text:
        current.append(char)
        if escape:
            escape = False
            continue
        if char == "\\":
            escape = True
            continue
        if char == "'" and not in_double:
            in_single = not in_single
        elif char == '"' and not in_single:
            in_double = not in_double
        elif char == ";" and not in_single and not in_double:
            statement = "".join(current).strip()
            if statement:
                statements.append(statement[:-1].strip())
            current = []

    tail = "".join(current).strip()
    if tail:
        statements.append(tail)
    return statements


def load_sql_file(path):
    text = path.read_text(encoding="utf-8-sig")
    lines = []
    for line in text.splitlines():
        stripped = line.strip()
        if stripped.startswith("--") or not stripped:
            continue
        lines.append(line)
    return "\n".join(lines)


def should_skip(statement, use_existing_database):
    normalized = statement.strip().lower()
    if use_existing_database and (
        normalized.startswith("create database")
        or normalized.startswith("use ")
        or normalized.startswith("drop database")
    ):
        return True
    return False


def main():
    parser = argparse.ArgumentParser(description="Import Math ARENA SQL files into Aiven MySQL.")
    parser.add_argument(
        "--url",
        default=os.getenv("AIVEN_MYSQL_URL"),
        help="mysql://USER:PASSWORD@HOST:PORT/DBNAME",
    )
    parser.add_argument(
        "--ca",
        default=os.getenv("AIVEN_CA_PATH"),
        help="Path to Aiven CA certificate file. Optional if SSL is not required.",
    )
    parser.add_argument(
        "--use-existing-database",
        action="store_true",
        help="Skip CREATE DATABASE and USE statements, importing into the database from the URL.",
    )
    parser.add_argument(
        "--sql",
        nargs="*",
        default=[str(path) for path in DEFAULT_SQL_FILES],
        help="SQL files to execute in order.",
    )
    parser.add_argument(
        "--ignore-duplicates",
        action="store_true",
        help="Skip duplicate primary-key rows while importing seed data.",
    )
    args = parser.parse_args()

    if not args.url:
        raise SystemExit("Set AIVEN_MYSQL_URL or pass --url.")

    config = parse_mysql_url(args.url)
    ssl = {"ca": args.ca} if args.ca else None
    connection = pymysql.connect(
        host=config["host"],
        port=config["port"],
        user=config["user"],
        password=config["password"],
        database=config["database"] if args.use_existing_database else None,
        charset="utf8mb4",
        autocommit=True,
        ssl=ssl,
        connect_timeout=15,
        read_timeout=60,
        write_timeout=60,
    )

    executed = 0
    try:
        with connection.cursor() as cursor:
            for sql_path in [Path(path) for path in args.sql]:
                print(f"==> {sql_path}")
                sql_text = load_sql_file(sql_path)
                for statement in split_sql_statements(sql_text):
                    if should_skip(statement, args.use_existing_database):
                        continue
                    try:
                        cursor.execute(statement)
                        executed += 1
                    except pymysql.err.IntegrityError as exc:
                        if args.ignore_duplicates and exc.args and exc.args[0] == 1062:
                            continue
                        raise
        print(f"Done. Executed {executed} SQL statements.")
    finally:
        connection.close()


if __name__ == "__main__":
    main()
