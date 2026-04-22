import datetime

import bcrypt
import jwt
from flask import current_app
from sqlalchemy import or_, select, text

from common.errors import AppError
from database import get_db
from models.profile import DEFAULT_TIER_NAME, Profile
from models.user import User
from services.tier_service import tier_name_to_api


def register_user(user_id, raw_password, nickname, email, phone, auth_id):
    db = get_db()

    existing = db.execute(
        select(User).where(
            or_(
                User.user_id == user_id,
                User.email == email,
                User.nickname == nickname,
                User.phone == phone,
            )
        )
    ).scalar_one_or_none()
    if existing:
        conflict_details = {
            "user_id": existing.user_id == user_id,
            "email": existing.email == email,
            "nickname": existing.nickname == nickname,
            "phone": existing.phone == phone,
        }
        raise AppError(
            message="duplicate user/email/nickname/phone",
            status=409,
            code="REGISTER_FAILED",
            details=conflict_details,
        )

    auth_row = db.execute(
        text(
            """
            SELECT auth_id, phone, is_verified, expires_at
            FROM PHONE_AUTH
            WHERE auth_id = :auth_id AND phone = :phone
            """
        ),
        {"auth_id": auth_id, "phone": phone},
    ).mappings().first()
    if not auth_row:
        raise AppError(message="phone auth request not found", status=404, code="REGISTER_FAILED")
    if not auth_row["is_verified"]:
        raise AppError(message="phone not verified", status=400, code="REGISTER_FAILED")
    if datetime.datetime.utcnow() > auth_row["expires_at"]:
        raise AppError(message="phone auth expired", status=400, code="REGISTER_FAILED")

    pw_hash = bcrypt.hashpw(raw_password.encode("utf-8"), bcrypt.gensalt()).decode("utf-8")

    new_user = User(
        user_id=user_id,
        email=email,
        password=pw_hash,
        nickname=nickname,
        phone=phone,
    )
    new_profile = Profile(
        user_id=user_id,
        nickname=nickname,
        tier_name=DEFAULT_TIER_NAME,
        arena_rating=0,
    )

    db.add(new_user)
    db.add(new_profile)
    db.commit()

    return {
        "user_id": user_id,
        "email": email,
        "nickname": nickname,
        "phone": phone,
    }



def login_user(user_id, raw_password):
    db = get_db()

    user = db.execute(select(User).where(User.user_id == user_id)).scalar_one_or_none()
    if not user:
        raise AppError(message="user not found", status=404, code="LOGIN_FAILED")

    if not bcrypt.checkpw(raw_password.encode("utf-8"), user.password.encode("utf-8")):
        raise AppError(message="invalid password", status=401, code="LOGIN_FAILED")

    profile = db.execute(select(Profile).where(Profile.user_id == user.user_id)).scalar_one_or_none()
    if not profile:
        raise AppError(message="profile not found", status=500, code="LOGIN_FAILED")

    payload = {
        "sub": user.user_id,
        "exp": datetime.datetime.utcnow() + datetime.timedelta(hours=24),
    }
    secret = current_app.config.get("JWT_SECRET", "secret")
    token = jwt.encode(payload, secret, algorithm="HS256")

    return {
        "access_token": token,
        "nickname": user.nickname,
        "tier": tier_name_to_api(profile.tier_name),
        "arena_rating": profile.arena_rating,
    }



def find_user_id_by_phone(auth_id, phone):
    db = get_db()

    auth_row = db.execute(
        text(
            """
            SELECT auth_id, phone, is_verified, expires_at
            FROM PHONE_AUTH
            WHERE auth_id = :auth_id AND phone = :phone
            """
        ),
        {"auth_id": auth_id, "phone": phone},
    ).mappings().first()

    if not auth_row:
        raise AppError(message="phone auth request not found", status=404, code="FIND_ID_FAILED")
    if not auth_row["is_verified"]:
        raise AppError(message="phone not verified", status=400, code="FIND_ID_FAILED")
    if datetime.datetime.utcnow() > auth_row["expires_at"]:
        raise AppError(message="phone auth expired", status=400, code="FIND_ID_FAILED")

    user = db.execute(select(User).where(User.phone == phone)).scalar_one_or_none()
    if not user:
        raise AppError(message="user not found", status=404, code="FIND_ID_FAILED")

    return {
        "user_id": user.user_id,
        "nickname": user.nickname,
        "phone": user.phone,
    }



def reset_user_password(user_id, nickname, new_password):
    db = get_db()

    user = db.execute(select(User).where(User.user_id == user_id)).scalar_one_or_none()
    if not user:
        raise AppError(message="user not found", status=404, code="RESET_PASSWORD_FAILED")
    if user.nickname != nickname:
        raise AppError(message="nickname does not match", status=400, code="RESET_PASSWORD_FAILED")

    pw_hash = bcrypt.hashpw(new_password.encode("utf-8"), bcrypt.gensalt()).decode("utf-8")
    user.password = pw_hash
    db.commit()

    return {
        "user_id": user.user_id,
        "nickname": user.nickname,
        "password_reset": True,
    }



def delete_user_account(user_id, raw_password):
    db = get_db()

    user = db.execute(select(User).where(User.user_id == user_id)).scalar_one_or_none()
    if not user:
        raise AppError(message="user not found", status=404, code="DELETE_ACCOUNT_FAILED")
    if not raw_password:
        raise AppError(message="pw is required", status=400, code="DELETE_ACCOUNT_FAILED")
    if not bcrypt.checkpw(raw_password.encode("utf-8"), user.password.encode("utf-8")):
        raise AppError(message="invalid password", status=401, code="DELETE_ACCOUNT_FAILED")

    nickname = user.nickname
    db.delete(user)
    db.commit()

    return {
        "user_id": user_id,
        "nickname": nickname,
        "deleted": True,
    }


