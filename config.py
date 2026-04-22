# config.py
import os
from dotenv import load_dotenv

def load_config(app):
    """환경 변수(.env) 로드 및 Flask 설정 등록"""
    # 1️⃣ .env 파일 불러오기
    load_dotenv()

    # 2️⃣ JWT & 인증 관련
    app.config["JWT_SECRET"] = os.getenv("JWT_SECRET", "dev-secret")
    app.config["AUTH_OFF"] = os.getenv("AUTH_OFF", "False").lower() == "true"
    app.config["DEV_TOKEN"] = os.getenv("DEV_TOKEN", "dev")

    # 3️⃣ 데이터베이스 (MySQL)
    db_user = os.getenv("MYSQL_USER", "arena_user")
    db_pw = os.getenv("MYSQL_PASSWORD", "arena_pw")
    db_host = os.getenv("MYSQL_HOST", "localhost")
    db_port = os.getenv("MYSQL_PORT", "3307")
    db_name = os.getenv("MYSQL_DATABASE", "arena")
    app.config["DB_URL"] = os.getenv(
        "DB_URL",
        f"mysql+pymysql://{db_user}:{db_pw}@{db_host}:{db_port}/{db_name}?charset=utf8mb4"
    )

    # 4️⃣ Redis
    app.config["REDIS_URL"] = os.getenv("REDIS_URL", "redis://localhost:6379/0")

    # 5️⃣ Flask 기본 옵션
    app.config["JSON_SORT_KEYS"] = False
    app.config["DEBUG"] = os.getenv("FLASK_DEBUG", "False").lower() == "true"
    app.config["FLASK_ENV"] = os.getenv("FLASK_ENV", "development")

    # 6️⃣ 앱 포트 (선택)
    app.config["APP_PORT"] = int(os.getenv("APP_PORT", 8000))
