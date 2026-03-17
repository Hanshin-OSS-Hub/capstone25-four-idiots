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
    VeryEasy = 1,  // 😊 Very Easy
    Easy     = 2,  // 🙂 Easy
    Hard     = 3,  // 😠 Hard
    VeryHard = 4,   // 😡 Very Hard (필요하면 더 추가)
    Tough    = 5,    // 추가
    VeryTough = 6    // 추가
}

public static class ExperienceSession
{
    public static ExperienceCategory CurrentCategory = ExperienceCategory.Concept;

    // 기본값은 VeryEasy
    public static ExperienceDifficulty CurrentDifficulty = ExperienceDifficulty.VeryEasy;
}
