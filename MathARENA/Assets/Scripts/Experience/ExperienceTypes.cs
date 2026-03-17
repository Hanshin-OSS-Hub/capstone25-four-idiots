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
    Easy     = 2,
    Hard     = 3,
    VeryHard = 4,
    Tough    = 5,
    VeryTough = 6
}

public static class ExperienceSession
{
    public static ExperienceCategory CurrentCategory = ExperienceCategory.Concept;
    public static ExperienceDifficulty CurrentDifficulty = ExperienceDifficulty.VeryEasy;

    // --- 아래 두 줄을 반드시 추가하세요 ---
    public static int TotalExpScore = 0;        
    public static int CurrentQuestionCount = 0; 
}