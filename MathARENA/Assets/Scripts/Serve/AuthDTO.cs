using System;

namespace MathArena.Network
{
    // --- 공통 응답 구조 ---
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

    // --- 로그인 (Login) ---
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
        public string tier;
        public int arena_rating;
    }

    // --- 회원가입 (Register) ---
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
}
