from flask_socketio import emit, join_room, leave_room
from flask import request

def register_socketio(socketio):
    # 클라이언트 연결 시
    @socketio.on("connect")
    def handle_connect():
        print(f"✅ Client connected: {request.sid}")
        emit("server_message", {"msg": "Connected to MathArena server!"})

    # 연결 해제 시
    @socketio.on("disconnect")
    def handle_disconnect():
        print(f"❌ Client disconnected: {request.sid}")

    # 방 참가
    @socketio.on("join_room")
    def handle_join(data):
        room = data.get("room")
        join_room(room)
        emit("server_message", {"msg": f"Joined room {room}"}, to=room)

    # 채팅 또는 데이터 전송 (테스트용)
    @socketio.on("chat")
    def handle_chat(data):
        msg = data.get("msg", "")
        room = data.get("room")
        emit("chat", {"sender": request.sid, "msg": msg}, to=room)

    # 아레나 매칭 (예시)
    @socketio.on("match_ready")
    def handle_match(data):
        player = data.get("player")
        emit("match_start", {"player": player, "msg": "Arena match started!"}, broadcast=True)
