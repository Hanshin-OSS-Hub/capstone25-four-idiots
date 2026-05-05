# api/auth.py
from functools import wraps

import jwt
from flask import Blueprint, current_app, g, jsonify, request

from common.errors import AppError
from common.responses import fail, ok
from services.auth import (
    delete_user_account,
    login_user,
    register_user,
    reset_user_password,
)

bp = Blueprint("auth", __name__, url_prefix="/v1/auth")


def require_user(f):
    @wraps(f)
    def wrapper(*args, **kwargs):
        auth = request.headers.get("Authorization", "")
        token = auth.split(" ", 1)[1].strip() if auth.startswith("Bearer ") else ""
        secret = current_app.config.get("JWT_SECRET", "dev-secret")

        if token:
            try:
                payload = jwt.decode(
                    token,
                    secret,
                    algorithms=["HS256"],
                    options={"verify_exp": False},
                )
                g.user_id = payload.get("sub")
                g.role = payload.get("role", "user")
            except jwt.InvalidTokenError:
                pass

        if not getattr(g, "user_id", None):
            body = request.get_json(silent=True) or {}
            g.user_id = (
                request.headers.get("X-User-Id")
                or body.get("user_id")
                or body.get("userId")
                or body.get("id")
                or request.args.get("user_id")
                or request.args.get("userId")
                or "dev"
            )
            g.role = "user"
        return f(*args, **kwargs)

    return wrapper


@bp.post("/find-id")
def find_id():
    return fail(
        "NOT_SUPPORTED",
        "find-id is no longer supported",
        410,
    )


@bp.post("/reset-password")
def reset_password():
    data = request.get_json(silent=True) or {}
    user_id = (data.get("id") or "").strip()
    nickname = (data.get("nickname") or "").strip()
    new_password = data.get("new_pw")
    new_password_confirm = data.get("new_pw_confirm")

    if not user_id or not nickname or not new_password or not new_password_confirm:
        return fail("BAD_REQUEST", "id, nickname, new_pw and new_pw_confirm are required", 400)
    if new_password != new_password_confirm:
        return fail("BAD_REQUEST", "new_pw and new_pw_confirm do not match", 400)

    try:
        return ok(
            reset_user_password(
                user_id=user_id,
                nickname=nickname,
                new_password=new_password,
            )
        )
    except AppError as e:
        return fail(e.code, str(e), e.status, e.details)
    except Exception as e:
        return fail("RESET_PASSWORD_FAILED", str(e), 500)


@bp.post("/delete-account")
@require_user
def delete_account():
    data = request.get_json(silent=True) or {}
    raw_password = data.get("pw")

    try:
        return ok(delete_user_account(user_id=g.user_id, raw_password=raw_password))
    except AppError as e:
        return fail(e.code, str(e), e.status, e.details)
    except Exception as e:
        return fail("DELETE_ACCOUNT_FAILED", str(e), 500)


@bp.post("/register")
def register():
    data = request.get_json(silent=True) or {}

    required_fields = ["id", "pw", "nickname", "email"]
    missing = [field for field in required_fields if not data.get(field)]
    if missing:
        return fail("BAD_REQUEST", f"missing fields: {', '.join(missing)}", 400)

    if data.get("pw_confirm") is not None and data["pw"] != data["pw_confirm"]:
        return fail("BAD_REQUEST", "pw and pw_confirm do not match", 400)

    try:
        result = register_user(
            user_id=data["id"],
            raw_password=data["pw"],
            nickname=data["nickname"],
            email=data["email"],
        )
        return ok(result)
    except AppError as e:
        return fail(e.code, str(e), e.status, e.details)
    except Exception as e:
        return fail("REGISTER_ERROR", str(e), 500)


@bp.post("/login")
def login():
    data = request.get_json(silent=True) or {}

    if not data.get("id") or not data.get("pw"):
        return fail("BAD_REQUEST", "id and pw are required.", 400)

    try:
        result = login_user(
            user_id=data["id"],
            raw_password=data["pw"],
        )
        return ok(result)
    except AppError as e:
        return fail(e.code, str(e), e.status, e.details)
    except Exception as e:
        return fail("LOGIN_FAILED", str(e), 401)


