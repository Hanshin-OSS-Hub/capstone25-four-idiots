# seed.py (새로 생성)
from app import create_app
from database import get_db
from models.problem import Problem
from sqlalchemy import text

app = create_app()

with app.app_context():
    db = get_db()
    
    print("🧹 기존 문제 데이터를 삭제합니다...")
    # 테이블 싹 비우기 (ID도 1번부터 다시 시작)
    db.execute(text("TRUNCATE TABLE problems"))
    
    print("📥 새 데이터를 넣습니다...")
    # 1. 연산 문제
    p1 = Problem(
        category='calculation', 
        difficulty='easy', 
        question_text='다음 이차방정식의 두 근의 합은?\n2x^2 + 6x + 3 = 0', 
        correct_answer='-3', 
        choices='-3, 3, -1.5, 1.5'
    )
    
    # 2. 개념이해 문제
    p2 = Problem(
        category='concept', 
        difficulty='easy', 
        question_text='다음 중 실수가 아닌 수는?', 
        correct_answer='√-1', 
        choices='0, -5, π, √-1'
    )
    
    db.add_all([p1, p2])
    db.commit()
    print("✅ 데이터 복구 완료! 이제 유니티에서 확인해보세요.")