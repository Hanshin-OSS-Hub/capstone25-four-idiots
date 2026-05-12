"""In-memory runtime state store.

This keeps training, experience, and arena runtime state in process memory.
It restores the pre-persistence behavior so request handlers stay fast and
stable during local development and demos.
"""

from __future__ import annotations

from typing import Any

_TRAINING_STATE: dict[str, dict[str, Any]] = {}
_EXPERIENCE_STATE: dict[str, dict[str, Any]] = {}
_ARENA_STATE: dict[str, dict[str, Any]] = {}


def ensure_runtime_state_table() -> None:
    """Compatibility no-op for app startup."""
    return


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


def _save(store: dict[str, dict[str, Any]], state_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    store[state_id] = _clone(payload)
    return _clone(store[state_id])


def _get(store: dict[str, dict[str, Any]], state_id: str) -> dict[str, Any] | None:
    return _clone(store.get(state_id))


def _pop(store: dict[str, dict[str, Any]], state_id: str) -> dict[str, Any] | None:
    return _clone(store.pop(state_id, None))


def create_training_session(session_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_TRAINING_STATE, session_id, payload)


def save_training_session(session_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_TRAINING_STATE, session_id, payload)


def get_training_session(session_id: str) -> dict[str, Any] | None:
    return _get(_TRAINING_STATE, session_id)


def pop_training_session(session_id: str) -> dict[str, Any] | None:
    return _pop(_TRAINING_STATE, session_id)


def create_experience_session(session_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_EXPERIENCE_STATE, session_id, payload)


def save_experience_session(session_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_EXPERIENCE_STATE, session_id, payload)


def get_experience_session(session_id: str) -> dict[str, Any] | None:
    return _get(_EXPERIENCE_STATE, session_id)


def pop_experience_session(session_id: str) -> dict[str, Any] | None:
    return _pop(_EXPERIENCE_STATE, session_id)


def create_arena_match(match_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_ARENA_STATE, match_id, payload)


def save_arena_match(match_id: str, payload: dict[str, Any]) -> dict[str, Any]:
    return _save(_ARENA_STATE, match_id, payload)


def get_arena_match(match_id: str) -> dict[str, Any] | None:
    return _get(_ARENA_STATE, match_id)


def pop_arena_match(match_id: str) -> dict[str, Any] | None:
    return _pop(_ARENA_STATE, match_id)
