-- =====================================================================================
-- MATH ARENA 물리적 데이터 모델링
-- =====================================================================================

-- 데이터베이스 생성 및 사용
CREATE DATABASE IF NOT EXISTS math_arena DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE math_arena;

-- 1. TIER (티어)
CREATE TABLE TIER (
    tier_name VARCHAR(50) NOT NULL COMMENT '티어명',
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
    password VARCHAR(255) NOT NULL COMMENT '비밀번호',
    nickname VARCHAR(50) NOT NULL UNIQUE COMMENT '닉네임 (Unique)',
    phone VARCHAR(20) NOT NULL UNIQUE COMMENT '전화번호 (Unique)',
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
    tier_name VARCHAR(50) NOT NULL DEFAULT '노멀 브론즈' COMMENT '티어명',
    arena_rating INT NOT NULL DEFAULT 0 COMMENT '아레나레이팅',
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '갱신일시',
    PRIMARY KEY (user_id),
    FOREIGN KEY (user_id) REFERENCES USER(user_id) ON DELETE CASCADE,
    FOREIGN KEY (tier_name) REFERENCES TIER(tier_name) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='사용자 프로필 및 전투력/티어 정보';

-- 5. PHONE_AUTH (휴대전화 인증 내역)
CREATE TABLE PHONE_AUTH (
    auth_id VARCHAR(50) NOT NULL COMMENT '인증 식별자 (UUID 등)',
    phone VARCHAR(20) NOT NULL COMMENT '전화번호',
    auth_code VARCHAR(10) NOT NULL COMMENT '인증번호',
    is_verified BOOLEAN NOT NULL DEFAULT FALSE COMMENT '인증완료여부',
    expires_at DATETIME NOT NULL COMMENT '인증만료시간 (요청 후 5분)',
    PRIMARY KEY (auth_id),
    INDEX idx_phone (phone)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='휴대전화 인증 정보';

-- =====================================================================================
-- 10. 아레나 매칭(Battle Matching) 성능 최적화를 위한 인덱스 
-- (오차범위 및 전투력 차이 최소화 검색용)
-- =====================================================================================
CREATE INDEX idx_profile_cp_concept ON PROFILE(cp_concept);
CREATE INDEX idx_profile_cp_calc ON PROFILE(cp_calc);
CREATE INDEX idx_profile_cp_idea ON PROFILE(cp_idea);
CREATE INDEX idx_profile_cp_design ON PROFILE(cp_design);
CREATE INDEX idx_profile_cp_practical ON PROFILE(cp_practical);

-- 6. 문제 테이블들 (종목별 분리)
-- 6-1. Q_CONCEPT (개념이해 문제)
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

-- 6-2. Q_CALC (연산 문제)
CREATE TABLE Q_CALC (
    q_id VARCHAR(50) NOT NULL COMMENT '연산 문제 식별자',
    category_name VARCHAR(50) NOT NULL DEFAULT '연산' COMMENT '종목명',
    diff_name VARCHAR(50) NOT NULL COMMENT '난이도명',
    content TEXT NOT NULL COMMENT '문제 내용',
    ocr_answer VARCHAR(255) NOT NULL COMMENT 'OCR답',
    PRIMARY KEY (q_id),
    FOREIGN KEY (diff_name) REFERENCES DIFFICULTY(diff_name) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='연산 종목 문제';

-- 6-3. Q_IDEA (발상 문제)
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

-- 6-4. Q_DESIGN (설계 문제)
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

-- 6-5. Q_PRACTICAL (실전 문제)
CREATE TABLE Q_PRACTICAL (
    q_id VARCHAR(50) NOT NULL COMMENT '실전 문제 식별자',
    category_name VARCHAR(50) NOT NULL DEFAULT '실전' COMMENT '종목명',
    diff_name VARCHAR(50) NOT NULL COMMENT '난이도명',
    content TEXT NOT NULL COMMENT '문제 내용',
    ocr_answer VARCHAR(255) NOT NULL COMMENT 'OCR답',
    PRIMARY KEY (q_id),
    FOREIGN KEY (diff_name) REFERENCES DIFFICULTY(diff_name) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='실전 종목 문제';

-- 7. RECORD_BATTLE_MATCH (훈련 세트 생성 및 아레나 상대방 기록 참조용)
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

-- 8. TRAINING_Q_SET_RECORD (세트 상세 기록 - 출제된 문제들)
CREATE TABLE TRAINING_Q_SET_RECORD (
    set_id VARCHAR(50) NOT NULL COMMENT '기록세트 식별자',
    question_order_number INT NOT NULL COMMENT '문제 출제 순서 번호',
    category_name VARCHAR(50) NOT NULL COMMENT '종목명',
    q_id VARCHAR(50) NOT NULL COMMENT '문제 식별자',
    presented_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '출제 일시',
    solve_time_sec INT NOT NULL COMMENT '풀이시간(초) - 최대 60초',
    is_correct BOOLEAN NOT NULL COMMENT '정답여부',
    PRIMARY KEY (set_id, question_order_number),
    FOREIGN KEY (set_id) REFERENCES RECORD_BATTLE_MATCH(set_id) ON DELETE CASCADE,
    INDEX idx_q_id (q_id) -- 문제 식별자 논리적 참조용 인덱스
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='기록 세트 내 출제된 문제 상세 기록';

-- 9. ARENA_PROGRESS (아레나 진행 상태 기록)
CREATE TABLE ARENA_PROGRESS (
    user_id VARCHAR(50) NOT NULL COMMENT '도전자 아이디',
    opponent_id VARCHAR(50) NOT NULL COMMENT '배틀상대 아이디',
    category_name VARCHAR(50) NOT NULL COMMENT '종목명',
    updated_cp INT NOT NULL COMMENT '배틀 기록 전투력',
    set_id VARCHAR(50) NOT NULL COMMENT '기록세트 식별자',
    current_q_id VARCHAR(50) COMMENT '아레나용 현재 문제 식별자',
    last_question_order INT NOT NULL DEFAULT 0 COMMENT '마지막 문제 순서 번호',
    PRIMARY KEY (user_id, opponent_id, category_name, updated_cp),
    FOREIGN KEY (user_id) REFERENCES USER(user_id) ON DELETE CASCADE,
    FOREIGN KEY (opponent_id) REFERENCES USER(user_id) ON DELETE CASCADE,
    FOREIGN KEY (set_id) REFERENCES RECORD_BATTLE_MATCH(set_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='아레나 배틀 중단 및 재개를 위한 진행 상황 저장';

-- =====================================================================================
--  11. 게임 로직 트랜잭션 및 동시성 제어 (Stored Procedure 실제 구현)
-- =====================================================================================

DELIMITER //

/*
[로직 1] 아레나 매칭 최적화 조회 프로시저
설명: 종목(category_name)에 따라 동적으로 해당 종목의 전투력을 기준으로 
      오차범위가 가장 적은 상대방을 추천합니다.
*/
CREATE PROCEDURE SP_GET_ARENA_MATCH(
    IN p_user_id VARCHAR(50),
    IN p_category_name VARCHAR(50),
    IN p_my_cp INT,
    IN p_limit INT
)
BEGIN
    IF p_category_name = '개념이해' THEN
        SELECT user_id, nickname, tier_name, cp_concept AS opponent_cp
        FROM PROFILE WHERE user_id != p_user_id
        ORDER BY ABS(cp_concept - p_my_cp) ASC LIMIT p_limit;
    ELSEIF p_category_name = '연산' THEN
        SELECT user_id, nickname, tier_name, cp_calc AS opponent_cp
        FROM PROFILE WHERE user_id != p_user_id
        ORDER BY ABS(cp_calc - p_my_cp) ASC LIMIT p_limit;
    ELSEIF p_category_name = '발상' THEN
        SELECT user_id, nickname, tier_name, cp_idea AS opponent_cp
        FROM PROFILE WHERE user_id != p_user_id
        ORDER BY ABS(cp_idea - p_my_cp) ASC LIMIT p_limit;
    ELSEIF p_category_name = '설계' THEN
        SELECT user_id, nickname, tier_name, cp_design AS opponent_cp
        FROM PROFILE WHERE user_id != p_user_id
        ORDER BY ABS(cp_design - p_my_cp) ASC LIMIT p_limit;
    ELSEIF p_category_name = '실전' THEN
        SELECT user_id, nickname, tier_name, cp_practical AS opponent_cp
        FROM PROFILE WHERE user_id != p_user_id
        ORDER BY ABS(cp_practical - p_my_cp) ASC LIMIT p_limit;
    END IF;
END //

/*
[로직 2] 훈련장 전투력 갱신 트랜잭션 프로시저 (원자성 및 동시성 보장)
설명: 비관적 락(Pessimistic Lock)을 통해 갱신 손실을 방지합니다.
      새로 달성한 전투력이 기존 전투력보다 높을 때만 프로필을 갱신하고,
      훈련 기록 세트(RECORD_BATTLE_MATCH)를 생성합니다.
*/
CREATE PROCEDURE SP_UPDATE_TRAINING_CP(
    IN p_user_id VARCHAR(50),
    IN p_nickname VARCHAR(50),
    IN p_category_name VARCHAR(50),
    IN p_new_cp INT,
    IN p_set_id VARCHAR(50),
    OUT p_is_updated BOOLEAN
)
BEGIN
    DECLARE v_current_cp INT DEFAULT 0;
    
    -- 예외 발생 시 트랜잭션 롤백 처리 핸들러
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_is_updated = FALSE;
    END;

    START TRANSACTION;

    -- 1. 비관적 락(FOR UPDATE)으로 해당 종목의 현재 전투력만 안전하게 읽기
    IF p_category_name = '개념이해' THEN
        SELECT cp_concept INTO v_current_cp FROM PROFILE WHERE user_id = p_user_id FOR UPDATE;
    ELSEIF p_category_name = '연산' THEN
        SELECT cp_calc INTO v_current_cp FROM PROFILE WHERE user_id = p_user_id FOR UPDATE;
    ELSEIF p_category_name = '발상' THEN
        SELECT cp_idea INTO v_current_cp FROM PROFILE WHERE user_id = p_user_id FOR UPDATE;
    ELSEIF p_category_name = '설계' THEN
        SELECT cp_design INTO v_current_cp FROM PROFILE WHERE user_id = p_user_id FOR UPDATE;
    ELSEIF p_category_name = '실전' THEN
        SELECT cp_practical INTO v_current_cp FROM PROFILE WHERE user_id = p_user_id FOR UPDATE;
    END IF;

    -- 2. 새로운 전투력이 더 높은 경우에만 갱신 로직 실행
    IF p_new_cp > v_current_cp THEN
        -- 2-1. 프로필 전투력 업데이트
        IF p_category_name = '개념이해' THEN
            UPDATE PROFILE SET cp_concept = p_new_cp WHERE user_id = p_user_id;
        ELSEIF p_category_name = '연산' THEN
            UPDATE PROFILE SET cp_calc = p_new_cp WHERE user_id = p_user_id;
        ELSEIF p_category_name = '발상' THEN
            UPDATE PROFILE SET cp_idea = p_new_cp WHERE user_id = p_user_id;
        ELSEIF p_category_name = '설계' THEN
            UPDATE PROFILE SET cp_design = p_new_cp WHERE user_id = p_user_id;
        ELSEIF p_category_name = '실전' THEN
            UPDATE PROFILE SET cp_practical = p_new_cp WHERE user_id = p_user_id;
        END IF;

        -- 2-2. 훈련 세트 메타데이터 기록
        -- (이 프로시저가 정상 동작하여 p_is_updated=TRUE를 반환하면 백엔드 앱에서 TRAINING_Q_SET_RECORD를 Batch Insert 합니다)
        INSERT INTO RECORD_BATTLE_MATCH (set_id, user_id, nickname, category_name, updated_cp, created_at)
        VALUES (p_set_id, p_user_id, p_nickname, p_category_name, p_new_cp, NOW());
        
        SET p_is_updated = TRUE;
    ELSE
        -- 갱신되지 않음
        SET p_is_updated = FALSE;
    END IF;

    COMMIT;
END //

/*
[로직 3] 아레나 결과 적용 프로시저 (동시성 제어)
설명: 아레나 종료 시 다중 기기 접속 등 어뷰징으로 인한 레이팅 복사(버그)를 
      방지하기 위해 락을 걸고 최종 AR과 티어를 갱신한 뒤, 아레나 진행 데이터를 삭제합니다.
*/
CREATE PROCEDURE SP_APPLY_ARENA_RESULT(
    IN p_user_id VARCHAR(50),
    IN p_opponent_id VARCHAR(50),
    IN p_category_name VARCHAR(50),
    IN p_final_ar INT,
    IN p_final_tier VARCHAR(50)
)
BEGIN
    DECLARE v_dummy_ar INT;

    -- 예외 발생 시 트랜잭션 롤백 처리 핸들러
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
    END;

    START TRANSACTION;

    -- 1. 비관적 락을 통한 무결성 보장 (트랜잭션이 끝날 때까지 다른 트랜잭션의 수정을 막음)
    SELECT arena_rating INTO v_dummy_ar FROM PROFILE WHERE user_id = p_user_id FOR UPDATE;

    -- 2. 백엔드에서 미리 승패/오차범위/모듈러 연산으로 계산된 최종 티어 및 AR 반영
    UPDATE PROFILE 
    SET arena_rating = p_final_ar, 
        tier_name = p_final_tier 
    WHERE user_id = p_user_id;

    -- 3. 아레나 진행 상태 삭제 (게임이 완전히 종료되었으므로 제거)
    DELETE FROM ARENA_PROGRESS 
    WHERE user_id = p_user_id 
      AND opponent_id = p_opponent_id 
      AND category_name = p_category_name;

    COMMIT;
END //

DELIMITER ;