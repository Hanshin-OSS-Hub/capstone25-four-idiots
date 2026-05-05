-- =====================================================================================
-- MATH ARENA 물리적 데이터 모델링
-- =====================================================================================
-- DROP DATABASE math_arena; 필요 시 주석삭제후 데이터베이스 삭제

-- 데이터베이스 생성 및 사용
CREATE DATABASE IF NOT EXISTS math_arena DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE math_arena;

-- 1. TIER (티어)
CREATE TABLE TIER (
    tier_name VARCHAR(50) NOT NULL COMMENT '티어명(Normal Bronze, Core Gold 등)',
    icon_url VARCHAR(255) COMMENT '티어 아이콘 이미지 URL',
    PRIMARY KEY (tier_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='티어 정보';

-- 2. DIFFICULTY (난이도)
CREATE TABLE DIFFICULTY (
    diff_name VARCHAR(50) NOT NULL COMMENT '난이도명 (VERY EASY, EASY, HARD, VERY HARD, TOUGH, VERY TOUGH)',
    score INT NOT NULL COMMENT '난이도별 점수 (5, 10, 15, 20, 25, 30)',
    icon_url VARCHAR(255) COMMENT '난이도 아이콘 이미지 URL',
    PRIMARY KEY (diff_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='문제 난이도 정보';

-- 3. USER (사용자)
CREATE TABLE USER (
    user_id VARCHAR(50) NOT NULL COMMENT '아이디',
    email VARCHAR(255) NOT NULL COMMENT '이메일',
    password VARCHAR(255) NOT NULL COMMENT '비밀번호(해시값 저장 권장)',
    nickname VARCHAR(50) NOT NULL UNIQUE COMMENT '닉네임 (Unique)',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '가입일시',
    PRIMARY KEY (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='사용자 계정 정보';

-- 4. PROFILE (프로필)
CREATE TABLE PROFILE (
    user_id VARCHAR(50) NOT NULL COMMENT '아이디',
    nickname VARCHAR(50) NOT NULL COMMENT '닉네임',
    cp_concept INT NOT NULL DEFAULT 0 COMMENT '개념이해전투력',
    cp_calc INT NOT NULL DEFAULT 0 COMMENT '연산전투력',
    cp_idea INT NOT NULL DEFAULT 0 COMMENT '발상전투력',
    cp_design INT NOT NULL DEFAULT 0 COMMENT '설계전투력',
    cp_practical INT NOT NULL DEFAULT 0 COMMENT '실전전투력',
    tier_name VARCHAR(50) NOT NULL DEFAULT 'Normal Bronze' COMMENT '티어명',
    arena_rating INT NOT NULL DEFAULT 0 COMMENT '아레나레이팅',
    PRIMARY KEY (user_id),
    FOREIGN KEY (user_id) REFERENCES USER(user_id) ON DELETE CASCADE,
    FOREIGN KEY (tier_name) REFERENCES TIER(tier_name) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='사용자 프로필 정보';

-- 5. 문제 테이블들 (종목별 분리)
-- 5-1. Q_CONCEPT (개념이해 문제)
CREATE TABLE Q_CONCEPT (
    q_id VARCHAR(50) NOT NULL COMMENT '개념이해 문제 식별자',
    category_name VARCHAR(50) NOT NULL DEFAULT '개념이해' COMMENT '종목명',
    diff_name VARCHAR(50) NOT NULL COMMENT '난이도명',
    content TEXT NOT NULL COMMENT '문제 내용',
    opt1 VARCHAR(255) NOT NULL COMMENT '보기1',
    opt2 VARCHAR(255) NOT NULL COMMENT '보기2',
    opt3 VARCHAR(255) NOT NULL COMMENT '보기3',
    opt4 VARCHAR(255) NOT NULL COMMENT '보기4',
    answer VARCHAR(255) NOT NULL COMMENT '보기답',
    PRIMARY KEY (q_id),
    FOREIGN KEY (diff_name) REFERENCES DIFFICULTY(diff_name) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='개념이해 종목 문제';

-- 5-2. Q_CALC (연산 문제)
CREATE TABLE Q_CALC (
    q_id VARCHAR(50) NOT NULL COMMENT '연산 문제 식별자',
    category_name VARCHAR(50) NOT NULL DEFAULT '연산' COMMENT '종목명',
    diff_name VARCHAR(50) NOT NULL COMMENT '난이도명',
    content TEXT NOT NULL COMMENT '문제 내용',
    ocr_answer VARCHAR(255) NOT NULL COMMENT 'OCR답',
    PRIMARY KEY (q_id),
    FOREIGN KEY (diff_name) REFERENCES DIFFICULTY(diff_name) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='연산 종목 문제';

-- 5-3. Q_IDEA (발상 문제)
CREATE TABLE Q_IDEA (
    q_id VARCHAR(50) NOT NULL COMMENT '발상 문제 식별자',
    category_name VARCHAR(50) NOT NULL DEFAULT '발상' COMMENT '종목명',
    diff_name VARCHAR(50) NOT NULL COMMENT '난이도명',
    content TEXT NOT NULL COMMENT '문제 내용',
    opt1 VARCHAR(255) NOT NULL COMMENT '보기1',
    opt2 VARCHAR(255) NOT NULL COMMENT '보기2',
    opt3 VARCHAR(255) NOT NULL COMMENT '보기3',
    opt4 VARCHAR(255) NOT NULL COMMENT '보기4',
    answer VARCHAR(255) NOT NULL COMMENT '보기답',
    PRIMARY KEY (q_id),
    FOREIGN KEY (diff_name) REFERENCES DIFFICULTY(diff_name) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='발상 종목 문제';

-- 5-4. Q_DESIGN (설계 문제)
CREATE TABLE Q_DESIGN (
    q_id VARCHAR(50) NOT NULL COMMENT '설계 문제 식별자',
    category_name VARCHAR(50) NOT NULL DEFAULT '설계' COMMENT '종목명',
    diff_name VARCHAR(50) NOT NULL COMMENT '난이도명',
    content TEXT NOT NULL COMMENT '문제 내용',
    opt1 VARCHAR(255) NOT NULL COMMENT '보기1',
    opt2 VARCHAR(255) NOT NULL COMMENT '보기2',
    opt3 VARCHAR(255) NOT NULL COMMENT '보기3',
    opt4 VARCHAR(255) NOT NULL COMMENT '보기4',
    order_answer VARCHAR(255) NOT NULL COMMENT '순서답 (예: 1-3-2-4)',
    PRIMARY KEY (q_id),
    FOREIGN KEY (diff_name) REFERENCES DIFFICULTY(diff_name) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='설계 종목 문제';

-- 5-5. Q_PRACTICAL (실전 문제)
CREATE TABLE Q_PRACTICAL (
    q_id VARCHAR(50) NOT NULL COMMENT '실전 문제 식별자',
    category_name VARCHAR(50) NOT NULL DEFAULT '실전' COMMENT '종목명',
    diff_name VARCHAR(50) NOT NULL COMMENT '난이도명',
    content TEXT NOT NULL COMMENT '문제 내용',
    ocr_answer VARCHAR(255) NOT NULL COMMENT 'OCR답',
    PRIMARY KEY (q_id),
    FOREIGN KEY (diff_name) REFERENCES DIFFICULTY(diff_name) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='실전 종목 문제';

-- 6. RECORD_BATTLE_MATCH (훈련 세트 생성 및 아레나 상대방 기록세트 참조)
CREATE TABLE RECORD_BATTLE_MATCH (
    set_id VARCHAR(50) NOT NULL COMMENT '기록세트 식별자',
    user_id VARCHAR(50) NOT NULL COMMENT '기록된 사용자 아이디',
    nickname VARCHAR(50) NOT NULL COMMENT '사용자 닉네임',
    category_name VARCHAR(50) NOT NULL COMMENT '종목명',
    updated_cp INT NOT NULL COMMENT '갱신 전투력',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '세트 생성 일시',
    PRIMARY KEY (set_id),
    FOREIGN KEY (user_id) REFERENCES USER(user_id) ON DELETE CASCADE,
    INDEX idx_user_category_cp (user_id, category_name, updated_cp) -- 매칭 시 빠른 조회를 위한 인덱스
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='훈련장 전투력 갱신 시 생성되는 기록 세트';

-- 7. TRAINING_Q_SET_RECORD (훈련장에서 출제된 문제들 기록 - 전투력 갱신 시 저장)
CREATE TABLE TRAINING_Q_SET_RECORD (
    set_id VARCHAR(50) NOT NULL COMMENT '기록세트 식별자',
    question_order_number INT NOT NULL COMMENT '문제 출제 순서 번호(1,2,3,...)',
    category_name VARCHAR(50) NOT NULL COMMENT '종목명',
    q_id VARCHAR(50) NOT NULL COMMENT '문제 식별자',
    presented_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '출제 일시',
    solve_time_sec INT NOT NULL COMMENT '풀이시간(초) - 최대 60초',
    is_correct BOOLEAN NOT NULL COMMENT '정답여부',
    PRIMARY KEY (set_id, question_order_number),
    FOREIGN KEY (set_id) REFERENCES RECORD_BATTLE_MATCH(set_id) ON DELETE CASCADE,
    INDEX idx_q_id (q_id) -- 문제 식별자 논리적 참조용 인덱스
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='기록 세트 내 출제된 문제 상세 기록';

-- 8. ARENA_PROGRESS (아레나 진행 상태 기록)
CREATE TABLE ARENA_PROGRESS (
    user_id VARCHAR(50) NOT NULL COMMENT '도전자 아이디',
    opponent_id VARCHAR(50) NOT NULL COMMENT '배틀상대 아이디',
    category_name VARCHAR(50) NOT NULL COMMENT '대결 종목명',
    updated_cp INT NOT NULL COMMENT '상대방 기록 전투력',
    set_id VARCHAR(50) NOT NULL COMMENT '상대방 기록세트 식별자',
    last_question_order INT NOT NULL DEFAULT 0 COMMENT '상대방 기록 최근 문제 순서 번호',
    PRIMARY KEY (user_id, opponent_id, category_name),
    FOREIGN KEY (user_id) REFERENCES USER(user_id) ON DELETE CASCADE,
    FOREIGN KEY (opponent_id) REFERENCES USER(user_id) ON DELETE CASCADE,
    FOREIGN KEY (set_id) REFERENCES RECORD_BATTLE_MATCH(set_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='사용자 간 아레나 대결 진행 상태 기록';

-- =====================================================================================
-- 9. 아레나 매칭(Battle Matching) 성능 최적화를 위한 인덱스
-- (오차범위 및 전투력 차이 최소화 검색용)
-- =====================================================================================
CREATE INDEX idx_profile_cp_concept ON PROFILE(cp_concept);
CREATE INDEX idx_profile_cp_calc ON PROFILE(cp_calc);
CREATE INDEX idx_profile_cp_idea ON PROFILE(cp_idea);
CREATE INDEX idx_profile_cp_design ON PROFILE(cp_design);
CREATE INDEX idx_profile_cp_practical ON PROFILE(cp_practical);
