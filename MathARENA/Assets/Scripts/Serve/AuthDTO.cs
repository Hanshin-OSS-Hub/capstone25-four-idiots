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

    [Serializable]
    public class RegisterRequest
    {
        public string id;
        public string pw;
        public string pw_confirm;
        public string nickname;
        public string email;
        public string phone;
        public string auth_id;
    }

    // 각 문제별 상세 기록
    [Serializable]
    public class QuestionResultData
    {
        public string q_id; // 문제 식별자
        public int solve_time_sec; // 풀이 시간
        public bool is_correct; // 정답 여부
    }

    // 최종 서버 전송용 DTO
    [Serializable]
    public class BattleResultRequest
    {
        public string category_name; // 종목명
        public int total_score; // 총 점수
        public List<QuestionResultData> results = new List<QuestionResultData>(); // 문제별 상세 기록
    }
}
