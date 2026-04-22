from sqlalchemy import DateTime, ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship
from sqlalchemy.sql import func, quoted_name

from .base import Base

DEFAULT_TIER_NAME = "normal bronze"


class Profile(Base):
    __tablename__ = quoted_name("PROFILE", True)

    user_id: Mapped[str] = mapped_column(
        String(50),
        ForeignKey("USER.user_id", ondelete="CASCADE"),
        primary_key=True,
    )
    nickname: Mapped[str] = mapped_column(String(50), nullable=False)
    cp_concept: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    cp_calc: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    cp_idea: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    cp_design: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    cp_practical: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    tier_name: Mapped[str] = mapped_column(String(50), nullable=False, default=DEFAULT_TIER_NAME)
    arena_rating: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    updated_at: Mapped[str] = mapped_column(
        DateTime,
        nullable=False,
        server_default=func.now(),
        onupdate=func.now(),
    )

    user = relationship("User", back_populates="profile")
