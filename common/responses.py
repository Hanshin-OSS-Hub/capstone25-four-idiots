from flask import jsonify, g

def ok(data=None, meta=None, status=200):
    return jsonify({
        "success": True,
        "data": data or {},
        "meta": {"request_id": getattr(g, "request_id", None), **(meta or {})}
    }), status

def fail(code: str, message: str, status: int, details=None):
    return jsonify({
        "success": False,
        "error": {"code": code, "message": message, "details": details or {}},
        "meta": {"request_id": getattr(g, "request_id", None)}
    }), status
