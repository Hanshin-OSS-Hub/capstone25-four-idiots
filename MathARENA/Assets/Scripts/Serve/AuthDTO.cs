using System;
using System.Collections.Generic;

namespace MathArena.Network
{
    [Serializable]
    public class AuthResponse<T>
    {
        public bool success;
        public T data;
        public ErrorData error;
    }

    [Serializable]
    public class ErrorData
    {
        public string code;
        public string message;
    }

    // [1] 세션 및 문제 데이터 관련
    [Serializable]
    public class ExperienceStartData
    {
        public string session_id;
        public string category;
        public string category_name;
        public int max_questions;
        public int remaining_questions;
    }

    [Serializable]
    public class ServerQuestionData
    {
        public string q_id;
        public string question_id;
        public string content;
        public string text;

        // public string answer; // 기존 필드 대신 아래 필드 확인
        public string correct_answer; // 서버에서 제공하는 정답 키 추가
        public int answer_val;
        public string choice;
        public List<int> answer_order;
        public List<string> choices;
        public string session_id;
    }

    // [2] 결과 제출 및 요청 관련
    [Serializable]
    public class QuestionResultData
    {
        public string question_id; // 서버가 요구하는 이름
        public int solve_time_sec;
        public string answer;

        public bool is_correct; // 제출 시에는 필요 없을 수 있음
        public string q_id; // 혹시 모르니 유지
    }

    [Serializable]
    public class BattleResultRequest
    {
        public string session_id = "";
        public string question_id = "";
        public string answer = "";
        public string answer_order = "";
        public string match_id = "";
        public string category_name = ""; // .ToLower() 변환 필수

        public int solve_time_sec;
        public bool is_correct;

        // [핵심 수정] total_score 대신 서버가 요구하는 updated_cp를 사용합니다.
        public int total_power;

        public List<QuestionResultData> results = new List<QuestionResultData>();
    }

    [Serializable]
    public class ExperienceSubmitResponse
    {
        public string session_id;
        public bool correct;
        public int earned_score;
        public bool finished;
    }

    // [3] 누락되었던 요청 클래스들 (에러 CS0246, CS0117 해결)
    [Serializable]
    public class MatchRequest
    {
        public string category;
        public string difficulty;
        public string match_id; // [추가] 이 자리가 있어야 서버로 ID가 전달됩니다.
    }

    [Serializable]
    public class RegisterRequest
    {
        public string id;
        public string pw;
        public string pw_confirm; // [추가] 에러 CS0117 해결용
        public string nickname;
        public string email; // [추가] 에러 CS0117 해결용
    }

    [Serializable]
    public class EmptyRequest { }

    [Serializable]
    public class QuestionRequest
    {
        public string category;
        public string difficulty;
        public string exclude_ids;
    }

    // [4] 유저 및 프로필 관련
    [Serializable]
    public class UserProfileData
    {
        public string user_id;
        public string nickname;

        // 전투력 5종 (ExperienceBattleController 220~230번 줄에서 참조)
        public int cp_concept;
        public int cp_calc;
        public int cp_idea;
        public int cp_design;
        public int cp_practical;

        // 티어 및 레이팅 (결과창 및 프로필 UI에서 참조)
        public string tier_name;
        public int arena_rating;

        // 로비 UI 호환용 (추가)
        public int gold;
        public int arenaTickets;
    }

    [Serializable]
    public class RankingEntryData
    {
        public string nickname;
        public string tier;
        public int arena_rating; // 서버 규격 (필수)

        // --- 아래는 UI 표시를 위해 필요한 추가 필드들입니다 ---
        public int rank;
        public int level;
        public int score;
        public UnityEngine.Sprite profileIcon;
    }

    [Serializable]
    public class LoginRequest
    {
        public string id;
        public string pw;
    }

    [Serializable]
    public class LoginData
    {
        public string access_token;
        public string nickname;
        public int arena_rating;
    }

    // 상대방의 훈련 기록 데이터 (TRAINING_Q_SET_RECORD 기반)
    [Serializable]
    public class OpponentRecord
    {
        public int question_order_number;
        public string q_id;
        public int solve_time_sec;
        public bool is_correct;
    }

    [Serializable]
    public class ArenaMatchCandidate
    {
        public string match_id;
        public string set_id;

        // [수정] 상대 정보가 중첩 객체로 들어오므로 구조를 맞춥니다.
        public OpponentData opponent;

        // [확인 필요] 로그에는 이 필드가 보이지 않습니다. 서버 팀에 확인이 필요합니다.
        public List<OpponentRecord> opponent_records;

        public int last_question_order;
    }

    [Serializable]
    public class ArenaMatchData
    {
        public List<ArenaMatchCandidate> candidates; // [수정] 단일 객체가 아닌 리스트로 변경
        public string status;
        public UserProfileData my_profile; // ← 이 줄 추가
    }

    // AuthDTO.cs [80번 줄 근처]
    [Serializable]
    public class ArenaStartData // [추가] 아레나 시작 시 내려오는 데이터 묶음
    {
        public string match_id;
        public List<ServerQuestionData> questions; // 핵심: 문제들이 리스트로 들어옵니다.
        public int my_lives;
        public int opponent_lives;
    }

    [Serializable]
    public class OpponentData // [새로 추가] 중첩된 상대 정보를 담는 클래스
    {
        public string id;
        public string nickname;
        public int power;
        public int arena_rating;
        public string tier;
    }
}
