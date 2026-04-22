using MathArena.Network;

public static class ExperienceSession
{
    // 배틀 상태 데이터
    public static int CurrentLife = 4;
    public static int MaxLife = 4;
    public static int TotalExpScore = 0;
    public static int CurrentQuestionCount = 0;
    public static ExperienceDifficulty CurrentDifficulty = ExperienceDifficulty.VeryEasy;

    // 유저 프로필 정보 (에러 로그의 'Profile' 이름과 맞춤)
    public static UserProfileData Profile;
}

// 난이도 구분을 위한 Enum
public enum ExperienceDifficulty
{
    VeryEasy,
    Easy,
    Hard,
    VeryHard,
    Tough,
    VeryTough,
}
