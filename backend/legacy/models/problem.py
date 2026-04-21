"""Legacy generic problem model.

The current production question flow uses the Q_CONCEPT/Q_CALC/Q_IDEA/
Q_DESIGN/Q_PRACTICAL tables directly. This model is kept only for older
seed scripts and backward compatibility.
"""

from sqlalchemy import Integer, String, Text
from sqlalchemy.orm import Mapped, mapped_column

from .base import Base


class Problem(Base):
    __tablename__ = "problems"

    id: Mapped[int] = mapped_column(primary_key=True, autoincrement=True)
    category: Mapped[str] = mapped_column(String(20))
    difficulty: Mapped[str] = mapped_column(String(10))
    question_text: Mapped[str] = mapped_column(Text)
    question_image: Mapped[str] = mapped_column(String(200), nullable=True)
    correct_answer: Mapped[str] = mapped_column(String(100))
    choices: Mapped[str] = mapped_column(String(200), nullable=True)
