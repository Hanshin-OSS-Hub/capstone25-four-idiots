using MathArena.Network;

/// <summary>
/// 체험장 및 전투 종목 정의
/// </summary>
public enum ExperienceCategory
{
    Concept, // 개념이해
    Calc, // 연산
    Idea, // 발상
    Design, // 설계
    Practice, // 실전
}

/// <summary>
/// 체험장 및 전투 전반에서 사용하는 난이도 정의
/// </summary>
public enum ExperienceDifficulty
{
    VeryEasy = 1,
    Easy = 2,
    Hard = 3,
    VeryHard = 4,
    Tough = 5,
    VeryTough = 6,
}

/// <summary>
/// 체험장 및 전투 세션 데이터를 관리하는 정적 클래스
/// </summary>
public static class ExperienceSession
{
    // --- 카테고리 및 난이도 ---
    public static ExperienceCategory CurrentCategory = ExperienceCategory.Concept;
    public static ExperienceDifficulty CurrentDifficulty = ExperienceDifficulty.VeryEasy;

    // --- 전투 진행 데이터 ---
    public static int TotalExpScore = 0; // 현재 배틀에서 획득한 총 점수
    public static int CurrentQuestionCount = 0; // 현재 풀고 있는 문제 번호

    // --- 생명력 및 타이머 (명세서 요구사항 반영) ---
    public static int CurrentLife = 4; // 기본 생명력 4개
    public const int MaxLife = 4; // 최대 생명력
    public const float QuestionTimeLimit = 60f; // 문제당 제한 시간 1분

    // --- 유저 정보 (서버 연동용) ---
    // ExperienceBattleController와의 호환성을 위해 UserProfileData 타입을 사용합니다.
    public static UserProfileData UserProfile;
}
