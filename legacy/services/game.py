from uuid import uuid4

QUESTIONS = {
    "q1": {
        "question_id": "q1",
        "text": "12 + 8 = ?",
        "choices": ["18", "20", "22", "24"],
        "answer": "20",
    },
    "q2": {
        "question_id": "q2",
        "text": "7 x 3 = ?",
        "choices": ["18", "20", "21", "24"],
        "answer": "21",
    },
}


def create_match(user_id, tier):
    opponent = f"bot_{tier}"
    return {
        "match_id": f"match-{uuid4().hex[:8]}",
        "room_id": f"room-{uuid4().hex[:8]}",
        "opponent": {
            "id": opponent,
            "nickname": opponent,
            "tier": tier,
        },
        "status": "matched",
    }


def start_match(match_id):
    selected = [QUESTIONS["q1"], QUESTIONS["q2"]]

    safe_questions = [
        {
            "question_id": q["question_id"],
            "text": q["text"],
            "choices": q["choices"],
        }
        for q in selected
    ]

    return {
        "match_id": match_id,
        "round_time": 30,
        "questions": safe_questions,
    }


def score_answer(qid, choice, time_ms):
    question = QUESTIONS.get(qid)
    if not question:
        return {"correct": False, "combo": 0, "round_score": 0}

    correct = str(choice) == str(question["answer"])
    base = 100 if correct else 0
    speed = max(0, 50 - (time_ms // 500))
    round_score = base + speed
    combo = 1 if correct else 0

    return {
        "correct": correct,
        "combo": combo,
        "round_score": int(round_score),
    }


def submit_answer(match_id, question_id, choice, time_ms, current_total=0):
    result = score_answer(question_id, choice, time_ms)
    total_score = current_total + result["round_score"]

    return {
        "match_id": match_id,
        "question_id": question_id,
        "correct": result["correct"],
        "earned_score": result["round_score"],
        "combo": result["combo"],
        "total_score": total_score,
    }


def finish_match(match_id, my_score, opponent_score):
    result = "win" if my_score > opponent_score else "lose"
    if my_score == opponent_score:
        result = "draw"

    reward = {
        "gold": 30 if result == "win" else 10,
        "xp": 20 if result == "win" else 5,
    }

    return {
        "match_id": match_id,
        "my_score": my_score,
        "opponent_score": opponent_score,
        "result": result,
        "reward": reward,
    }
