"""In-memory runtime state store.

This keeps training, experience, and arena runtime state in process memory.
It restores the pre-persistence behavior so request handlers stay fast and
stable during local development and demos.
"""

from __future__ import annotations

from copy import deepcopy
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
    return deepcopy(payload)


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
