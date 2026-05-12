from flask import Flask, app, request, g, jsonify
from flask_socketio import SocketIO
from flask_cors import CORS
from flask_limiter import Limiter
from flask_limiter.util import get_remote_address
import logging, uuid, time
from werkzeug.exceptions import HTTPException

# --------------------------------------------
# 1. ?꾩닔 紐⑤뱢 ?꾪룷??
# --------------------------------------------
from common.responses import ok, fail
from common.errors import AppError
from config import load_config
from database import get_db, init_db
from services.question_service import ensure_user_solved_question_table
from services.runtime_state import ensure_runtime_state_table

# --------------------------------------------
# 2. ?좏깮??紐⑤뱢 ?꾪룷??(?덉쇅 泥섎━)
# --------------------------------------------
try:
    from api import register_blueprints
except ImportError:
    print("Optional module 'api' is not available.")
    def register_blueprints(app): pass

try:
    from sockets.arena import register_socketio
except ImportError:
    print("Optional module 'sockets.arena' is not available.")
    def register_socketio(socketio): pass

# --------------------------------------------
# 3. SocketIO 媛앹껜 ?앹꽦 (?꾩뿭)
# --------------------------------------------
# threading 紐⑤뱶??媛쒕컻?⑹엯?덈떎. 諛고룷 ?쒖뿏 eventlet ?깆쓣 沅뚯옣?⑸땲??
socketio = SocketIO(cors_allowed_origins="*", async_mode="threading")

# --------------------------------------------
# 4. Flask App Factory
# --------------------------------------------
def create_app():
    app = Flask(__name__)
    
    # 1截뤴깵 ?ㅼ젙 濡쒕뱶
    load_config(app)

    # 2截뤴깵 ?곗씠?곕쿋?댁뒪 珥덇린??(媛??癒쇱?)
    with app.app_context():
        try:
            init_db(app)
            ensure_user_solved_question_table(get_db())
            ensure_runtime_state_table()
        except Exception as e:
            app.logger.error(f"??DB 珥덇린???ㅽ뙣: {str(e)}")

    # 3截뤴깵 DB ?몄뀡 ?뺣━ (?꾩닔)
    @app.teardown_appcontext
    def shutdown_session(exception=None):
        try:
            get_db().remove()
        except Exception:
            pass

    # 4截뤴깵 CORS ?ㅼ젙
    CORS(app, resources={r"/*": {"origins": "*"}})

    # 5截뤴깵 SocketIO ???곌껐 (?ш린媛 ?듭떖 ?섏젙 ?ы빆)
    # create_app ?덉뿉??init_app???댁빞 ?덉쟾?섍쾶 ?곌껐?⑸땲??
    socketio.init_app(app, cors_allowed_origins="*", async_mode="threading")

    # ----------------------------------------
    # ?お 媛쒕컻???몄쬆 ?고쉶
    # ----------------------------------------
    app.config["DEV_TOKEN"] = app.config.get("DEV_TOKEN", "dev")



    # def dev_auth_bypass():
    #     if request.path == "/healthz" or request.method == "OPTIONS":
    #         return
    #     if app.config.get("AUTH_OFF", False):
    #         g.user_id = "dev"
    #         return
    #     auth = request.headers.get("Authorization", "")
    #     if auth.startswith("Bearer "):
    #         token = auth.split(" ", 1)[1].strip()
    #         if token in {"dev", app.config.get("DEV_TOKEN", "")}:
    #             g.user_id = "dev"
    #             return

    # app.before_request_funcs.setdefault(None, []).insert(0, dev_auth_bypass) 

    # ----------------------------------------
    # ?숋툘 Rate Limiter & Logging
    # ----------------------------------------
    limiter = Limiter(
        get_remote_address,
        app=app,
        default_limits=["100 per minute"],
        storage_uri="memory://"
    )

    logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
    log = logging.getLogger("server")
    logging.getLogger("werkzeug").setLevel(logging.WARNING)

    @app.before_request
    def _rid():
        g.request_id = request.headers.get("X-Request-Id", str(uuid.uuid4()))
        g.start_ts = time.time()

    @app.after_request
    def _log_response(resp):
        dur = int((time.time() - g.get("start_ts", time.time())) * 1000)
        if request.path != "/healthz": # ?ъ뒪泥댄겕 濡쒓렇???덈Т 留롮쑝???앸왂 媛??
            log.info(f"rid={g.request_id} {request.method} {request.path} {resp.status_code} {dur}ms")
        return resp

    # ----------------------------------------
    # ?좑툘 ?먮윭 ?몃뱾留?
    # ----------------------------------------
    @app.errorhandler(AppError)
    def _app_error(e: AppError):
        return fail(e.code, str(e), e.status, e.details)

    @app.errorhandler(HTTPException)
    def _http_error(e: HTTPException):
        return fail(e.name.upper().replace(" ", "_"), e.description, e.code)

    @app.errorhandler(Exception)
    def _unhandled(e: Exception):
        app.logger.exception("Unhandled exception")
        return fail("INTERNAL_ERROR", "Unexpected server error", 500)

    # ----------------------------------------
    # ?뱻 ?쇱슦???깅줉
    # ----------------------------------------
    register_blueprints(app)
    register_socketio(socketio)

    # ----------------------------------------
    # ?㈉ ?ъ뒪泥댄겕
    # ----------------------------------------
    @app.get("/healthz")
    @limiter.exempt
    def healthz():
        status = "ok"
        try:
            from sqlalchemy import text
            get_db().execute(text("SELECT 1"))
        except Exception:
            status = "db_error"
        return ok({"status": status})

    return app

# --------------------------------------------
# ?? ?ㅽ뻾遺
# --------------------------------------------
app = create_app()

if __name__ == "__main__":
    import os

    socketio.run(
        app,
        host="0.0.0.0",
        port=int(os.getenv("PORT", 8001)),
        debug=False,
        allow_unsafe_werkzeug=True,
    )
