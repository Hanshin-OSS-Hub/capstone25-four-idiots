using UnityEngine;

public enum ExperienceCategory
{
    Concept, // 개념이해
    Calc, // 연산
    Idea, // 발상
    Design, // 설계
    Practice, // 실전
}

public enum ExperienceDifficulty
{
    VeryEasy = 1,
    Easy = 2,
    Hard = 3,
    VeryHard = 4,
    Tough = 5,
    VeryTough = 6,
}

public static class ExperienceSession
{
    public static ExperienceCategory CurrentCategory = ExperienceCategory.Concept;
    public static ExperienceDifficulty CurrentDifficulty = ExperienceDifficulty.VeryEasy;

    public static int TotalExpScore = 0;
    public static int CurrentQuestionCount = 0;

    // --- 생명력 및 타이머 관련 데이터 ---
    public static int CurrentLife = 4; // 기본 생명력 4개
    public const int MaxLife = 4;
    public const float QuestionTimeLimit = 60f; // 문제당 1분 (60초)
}
