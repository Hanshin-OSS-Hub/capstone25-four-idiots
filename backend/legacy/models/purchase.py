"""Legacy purchase model.

Not used by the current Math Arena core game flow.
"""

from sqlalchemy import JSON, String, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column

from .base import Base


class Purchase(Base):
    __tablename__ = "purchases"

    id: Mapped[int] = mapped_column(primary_key=True, autoincrement=True)
    uid: Mapped[str] = mapped_column(String(64))
    provider: Mapped[str] = mapped_column(String(16))
    receipt_id: Mapped[str] = mapped_column(String(128))
    payload: Mapped[dict] = mapped_column(JSON, default={})

    __table_args__ = (UniqueConstraint("provider", "receipt_id", name="u_receipt"),)
