"""Database-backed runtime state store.

This persists training, experience, and arena runtime state in the shared DB so
session/match state survives Render restarts and instance changes.
"""

from __future__ import annotations

import json
from typing import Any

from sqlalchemy import text

from database import get_db

_STATE_TYPE_TRAINING = "training"
_STATE_TYPE_EXPERIENCE = "experience"
_STATE_TYPE_ARENA = "arena"


def ensure_runtime_state_table() -> None:
    db = get_db()
    db.execute(
        text(
            """
            CREATE TABLE IF NOT EXISTS RUNTIME_STATE (
                state_id VARCHAR(64) NOT NULL,
                state_type VARCHAR(32) NOT NULL,
                payload LONGTEXT NOT NULL,
                updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                    ON UPDATE CURRENT_TIMESTAMP,
                PRIMARY KEY (state_id),
                INDEX idx_runtime_state_type (state_type)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
            """
        )
    )
    db.commit()


def _clone(payload: dict[str, Any] | None) -> dict[str, Any] | None:
    if payload is None:
        return None

    cloned = dict(payload)

    if "asked_ids" in cloned and isinstance(cloned["asked_ids"], list):
        cloned["asked_ids"] = list(cloned["asked_ids"])
    if "served_answers" in cloned and isinstance(cloned["served_answers"], dict):
        cloned["served_answers"] = dict(cloned["served_answers"])
    if "records" in cloned and isinstance(cloned["records"], list):
        cloned["records"] = [
            dict(item) if isinstance(item, dict) else item
            for item in cloned["records"]
        ]
    if "questions" in cloned and isinstance(cloned["questions"], dict):
        cloned["questions"] = {
            key: dict(value) if isinstance(value, dict) else value
            for key, value in cloned["questions"].items()
        }
    if "question_order" in cloned and isinstance(cloned["question_order"], list):
        cloned["question_order"] = list(cloned["question_order"])
    if "question_number_map" in cloned and isinstance(cloned["question_number_map"], dict):
        cloned["question_number_map"] = dict(cloned["question_number_map"])
    if "answered" in cloned and isinstance(cloned["answered"], set):
        cloned["answered"] = set(cloned["answered"])

    return cloned


def _json_safe(value: Any) -> Any:
    if isinstance(value, dict):
        return {str(key): _json_safe(item) for key, item in value.items()}
    if isinstance(value, list):
        return [_json_safe(item) for item in value]
    if isinstance(value, set):
        return sorted(_json_safe(item) for item in value)
    return value


def _restore_payload(payload: dict[str, Any] | None) -> dict[str, Any] | None:
    restored = _clone(payload)
    if restored is None:
        return None
    if "answered" in restored and isinstance(restored["answered"], list):
        restored["answered"] = set(restored["answered"])
    return restored


def _save(state_type: str, state_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    db = get_db()
    serialized = json.dumps(_json_safe(payload), ensure_ascii=False)
    db.execute(
        text(
            """
            INSERT INTO RUNTIME_STATE (state_id, state_type, payload)
            VALUES (:state_id, :state_type, :payload)
            ON DUPLICATE KEY UPDATE
                state_type = VALUES(state_type),
                payload = VALUES(payload)
            """
        ),
        {
            "state_id": state_id,
            "state_type": state_type,
            "payload": serialized,
        },
    )
    db.commit()
    return _restore_payload(payload)


def _get(state_type: str, state_id: str) -> dict[str, Any] | None:
    db = get_db()
    row = db.execute(
        text(
            """
            SELECT payload
            FROM RUNTIME_STATE
            WHERE state_id = :state_id
              AND state_type = :state_type
            """
        ),
        {"state_id": state_id, "state_type": state_type},
    ).mappings().first()
    if not row:
        return None
    return _restore_payload(json.loads(row["payload"]))


def _pop(state_type: str, state_id: str) -> dict[str, Any] | None:
    payload = _get(state_type, state_id)
    if payload is None:
        return None

    db = get_db()
    db.execute(
        text(
            """
            DELETE FROM RUNTIME_STATE
            WHERE state_id = :state_id
              AND state_type = :state_type
            """
        ),
        {"state_id": state_id, "state_type": state_type},
    )
    db.commit()
    return payload


def create_training_session(session_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_STATE_TYPE_TRAINING, session_id, payload)


def save_training_session(session_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_STATE_TYPE_TRAINING, session_id, payload)


def get_training_session(session_id: str) -> dict[str, Any] | None:
    return _get(_STATE_TYPE_TRAINING, session_id)


def pop_training_session(session_id: str) -> dict[str, Any] | None:
    return _pop(_STATE_TYPE_TRAINING, session_id)


def create_experience_session(session_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_STATE_TYPE_EXPERIENCE, session_id, payload)


def save_experience_session(session_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_STATE_TYPE_EXPERIENCE, session_id, payload)


def get_experience_session(session_id: str) -> dict[str, Any] | None:
    return _get(_STATE_TYPE_EXPERIENCE, session_id)


def pop_experience_session(session_id: str) -> dict[str, Any] | None:
    return _pop(_STATE_TYPE_EXPERIENCE, session_id)


def create_arena_match(match_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_STATE_TYPE_ARENA, match_id, payload)


def save_arena_match(match_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_STATE_TYPE_ARENA, match_id, payload)


def get_arena_match(match_id: str) -> dict[str, Any] | None:
    return _get(_STATE_TYPE_ARENA, match_id)


def pop_arena_match(match_id: str) -> dict[str, Any] | None:
    return _pop(_STATE_TYPE_ARENA, match_id)
