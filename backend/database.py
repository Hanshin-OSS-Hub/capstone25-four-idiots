import os
import tempfile

from sqlalchemy import create_engine
from sqlalchemy.orm import scoped_session, sessionmaker

from models.base import Base


db_session = None
_temp_ssl_files = []


def _write_temp_ssl_file(pem_text, suffix):
    fd, path = tempfile.mkstemp(prefix="math_arena_db_ssl_", suffix=suffix)
    with os.fdopen(fd, "w", encoding="utf-8") as handle:
        handle.write(pem_text)
    _temp_ssl_files.append(path)
    return path


def _build_connect_args(app):
    ssl_args = {}

    ca_path = app.config.get("DB_SSL_CA_PATH")
    if not ca_path:
        ca_pem = app.config.get("DB_SSL_CA_PEM")
        if ca_pem:
            ca_path = _write_temp_ssl_file(ca_pem, ".pem")

    cert_path = app.config.get("DB_SSL_CERT_PATH")
    key_path = app.config.get("DB_SSL_KEY_PATH")

    if ca_path:
        ssl_args["ca"] = ca_path
    if cert_path:
        ssl_args["cert"] = cert_path
    if key_path:
        ssl_args["key"] = key_path

    if ssl_args:
        app.logger.info("Database SSL is enabled")
        return {"ssl": ssl_args}

    return {}


def init_db(app):
    global db_session

    database_url = app.config.get("DB_URL")
    if not database_url:
        raise ValueError("DB_URL is not configured. Check your environment variables.")

    connect_args = _build_connect_args(app)

    engine = create_engine(
        database_url,
        echo=app.config.get("DEBUG", False),
        pool_pre_ping=True,
        connect_args=connect_args,
    )

    db_session = scoped_session(
        sessionmaker(autocommit=False, autoflush=False, bind=engine)
    )
    Base.query = db_session.query_property()

    import models

    Base.metadata.create_all(bind=engine)
    app.logger.info("Database connection initialized")


def get_db():
    if db_session is None:
        raise RuntimeError("Database session is not initialized")
    return db_session
