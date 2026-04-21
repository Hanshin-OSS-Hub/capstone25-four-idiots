"""Legacy mission model.

Not used by the current Math Arena core game flow.
"""

from sqlalchemy import Date, JSON, String, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column

from .base import Base


class Mission(Base):
    __tablename__ = "missions"

    id: Mapped[int] = mapped_column(primary_key=True, autoincrement=True)
    uid: Mapped[str] = mapped_column(String(64))
    day: Mapped[str] = mapped_column(Date)
    tasks: Mapped[dict] = mapped_column(JSON, default={})
    progress: Mapped[dict] = mapped_column(JSON, default={})

    __table_args__ = (UniqueConstraint("uid", "day", name="u_uid_day"),)
