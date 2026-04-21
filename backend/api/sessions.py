from flask import Blueprint, g

from common.responses import ok
from .auth import require_user

bp = Blueprint("sessions", __name__, url_prefix="/v1/sessions")


@bp.get("/me")
@require_user
def get_session_info():
    return ok(
        {
            "user_id": g.get("user_id"),
            "role": g.get("role", "user"),
            "status": "active",
        }
    )


@bp.post("/logout")
@require_user
def logout():
    # JWT is stateless in the current server, so logout is a client-side token discard.
    return ok(
        {
            "user_id": g.get("user_id"),
            "logged_out": True,
            "message": "token should be discarded on the client",
        }
    )
