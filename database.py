from sqlalchemy import create_engine
import os
import tempfile

from sqlalchemy.orm import scoped_session, sessionmaker

from models.base import Base


db_session = None


def _build_connect_args():
    ca_pem = os.getenv("DB_SSL_CA_PEM")
    if not ca_pem:
        return {}

    ca_path = os.path.join(tempfile.gettempdir(), "aiven-ca.pem")
    with open(ca_path, "w", encoding="utf-8") as ca_file:
        ca_file.write(ca_pem.replace("\\n", "\n"))

    return {"ssl": {"ca": ca_path}}


def init_db(app):
    global db_session

    database_url = app.config.get("DB_URL")
    if not database_url:
        raise ValueError("DB_URL is not configured. Check your environment variables.")

    engine = create_engine(
        database_url,
        echo=app.config.get("DEBUG", False),
        pool_pre_ping=True,
        connect_args=_build_connect_args(),
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
