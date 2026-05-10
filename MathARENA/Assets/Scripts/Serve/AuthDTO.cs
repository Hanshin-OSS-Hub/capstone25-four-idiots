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
        public string session_id;
        public string category_name;
        public int total_score;
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
        public string nickname;
        public string tier;
        public int arena_rating;
        public int cp_concept;
        public int cp_calc;
        public int cp_idea;
        public int cp_design;
        public int cp_practical;
        public string email;
    }

    [Serializable]
    public class RankingEntryData
    {
        public string nickname;
        public string tier;
        public int arena_rating;
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
}
