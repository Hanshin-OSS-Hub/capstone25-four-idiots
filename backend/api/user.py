from flask import Blueprint, g
from sqlalchemy import select, text

from common.responses import fail, ok
from database import get_db
from models.profile import DEFAULT_TIER_NAME, Profile
from models.user import User
from services.tier_service import split_tier_name, tier_name_to_api
from .auth import require_user

bp = Blueprint("user", __name__, url_prefix="/v1/user")


def _average_power(profile):
    values = [
        int(profile.cp_concept or 0),
        int(profile.cp_calc or 0),
        int(profile.cp_idea or 0),
        int(profile.cp_design or 0),
        int(profile.cp_practical or 0),
    ]
    return int(sum(values) / len(values))


@bp.get("/profile")
@require_user
def get_profile():
    try:
        db = get_db()
        user = db.execute(select(User).where(User.user_id == g.user_id)).scalar_one_or_none()
        profile = db.execute(select(Profile).where(Profile.user_id == g.user_id)).scalar_one_or_none()

        if not user or not profile:
            return fail("USER_NOT_FOUND", "user/profile not found", 404)

        tier_name = profile.tier_name or DEFAULT_TIER_NAME
        api_tier_name = tier_name_to_api(tier_name)
        tier_parts = split_tier_name(tier_name)
        tier_row = db.execute(
            text(
                """
                SELECT icon_url
                FROM TIER
                WHERE tier_name = :tier_name
                """
            ),
            {"tier_name": tier_name},
        ).mappings().first()
        tier_icon_url = tier_row["icon_url"] if tier_row else None

        powers = {
            "concept": int(profile.cp_concept or 0),
            "calc": int(profile.cp_calc or 0),
            "idea": int(profile.cp_idea or 0),
            "design": int(profile.cp_design or 0),
            "practice": int(profile.cp_practical or 0),
            "average": _average_power(profile),
        }

        return ok(
            {
                "user_id": user.user_id,
                "email": user.email,
                "nickname": user.nickname,
                "tier": {
                    "name": api_tier_name,
                    "base": tier_parts["base"],
                    "stage": tier_parts["stage"],
                    "stage_index": None if tier_parts["stage_index"] is None else int(tier_parts["stage_index"]) + 1,
                    "base_index": None if tier_parts["base_index"] is None else int(tier_parts["base_index"]) + 1,
                    "arena_rating": int(profile.arena_rating or 0),
                    "icon_url": tier_icon_url,
                    "icon_key": api_tier_name,
                },
                "powers": powers,
                # Backward-compatible flat fields for existing clients.
                "arena_rating": int(profile.arena_rating or 0),
                "tier_name": api_tier_name,
                "tier_base": tier_parts["base"],
                "tier_stage": tier_parts["stage"],
                "tier_stage_index": None if tier_parts["stage_index"] is None else int(tier_parts["stage_index"]) + 1,
                "tier_icon_url": tier_icon_url,
                "tier_icon_key": api_tier_name,
                "cp_concept": powers["concept"],
                "cp_calc": powers["calc"],
                "cp_idea": powers["idea"],
                "cp_design": powers["design"],
                "cp_practical": powers["practice"],
                "average_power": powers["average"],
            }
        )
    except Exception as e:
        return fail("SERVER_ERROR", str(e), 500)


