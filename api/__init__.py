from .auth import bp as auth_bp
from .experience import bp as experience_bp
from .match import bp as match_bp
from .sessions import bp as sessions_bp
from .training import bp as training_bp
from .user import bp as user_bp


def register_blueprints(app):
    app.register_blueprint(auth_bp)
    app.register_blueprint(user_bp)
    app.register_blueprint(match_bp)
    app.register_blueprint(sessions_bp)
    app.register_blueprint(training_bp)
    app.register_blueprint(experience_bp)
