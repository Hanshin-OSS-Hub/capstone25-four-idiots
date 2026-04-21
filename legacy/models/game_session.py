"""Legacy session model kept only for backward compatibility.

Runtime session state is now persisted through services.runtime_state
and the RUNTIME_SESSION_STATE table instead of this ORM model.
"""

from sqlalchemy import DateTime, Enum as SqEnum, Integer, JSON, String
from sqlalchemy.orm import Mapped, mapped_column
import enum

from models.base import Base


class SessionState(enum.Enum):
    match = "match"
    play = "play"
    end = "end"


class GameSession(Base):
    __tablename__ = "sessions"

    id: Mapped[int] = mapped_column(primary_key=True, autoincrement=True)
    session_id: Mapped[str] = mapped_column(String(40), unique=True)
    state: Mapped[SessionState] = mapped_column(SqEnum(SessionState))
    participants: Mapped[dict] = mapped_column(JSON)
    started_at: Mapped[str] = mapped_column(DateTime)
    remaining_ms: Mapped[int] = mapped_column(Integer, default=90000)
    domain: Mapped[str] = mapped_column(String(24))
    diff: Mapped[int] = mapped_column(Integer, default=1)
