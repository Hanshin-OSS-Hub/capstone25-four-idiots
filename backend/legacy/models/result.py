"""Legacy result model.

Not used by the current Math Arena core game flow.
"""

from sqlalchemy import Integer, JSON, String, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column

from .base import Base


class Result(Base):
    __tablename__ = "results"

    id: Mapped[int] = mapped_column(primary_key=True, autoincrement=True)
    session_id: Mapped[str] = mapped_column(String(40))
    uid: Mapped[str] = mapped_column(String(64))
    correct: Mapped[int] = mapped_column(Integer, default=0)
    wrong: Mapped[int] = mapped_column(Integer, default=0)
    combo_max: Mapped[int] = mapped_column(Integer, default=0)
    time_mean: Mapped[int] = mapped_column(Integer, default=0)
    domain_stats: Mapped[dict] = mapped_column(JSON, default={})

    __table_args__ = (UniqueConstraint("session_id", "uid", name="u_sess_uid"),)
