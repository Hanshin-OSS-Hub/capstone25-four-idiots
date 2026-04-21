# api/store.py
from flask import Blueprint, request, jsonify, g
from .auth import require_user

bp = Blueprint("store", __name__, url_prefix="/v1/store")

# 데모용 상점 데이터
ITEMS = {
    "costume_basic": {"price": 100, "name": "기본 코스튬"},
    "arena_ticket": {"price": 50, "name": "아레나 티켓"},
}


@bp.get("/list")
@require_user
def list_items():
    """
    ✅ 상점 아이템 목록 조회
    Response:
      { "items": [ {id, name, price}, ... ] }
    """
    item_list = [{"id": k, **v} for k, v in ITEMS.items()]
    return jsonify(items=item_list)


@bp.post("/buy")
@require_user
def buy_item():
    """
    ✅ 아이템 구매
    Body:
      { "item_id": "arena_ticket", "balance": 200 }
    Response:
      { "result": "success", "remaining": 150 }
    """
    data = request.get_json(silent=True) or {}
    item_id = data.get("item_id")
    balance = data.get("balance", 0)

    if not item_id or item_id not in ITEMS:
        return jsonify(error="invalid item_id"), 400
    try:
        balance = int(balance)
    except ValueError:
        return jsonify(error="balance must be int"), 400

    price = ITEMS[item_id]["price"]
    if balance < price:
        return jsonify(error="insufficient balance"), 400

    remaining = balance - price
    return jsonify(result="success", item=item_id, remaining=remaining)
