using UnityEngine;

public static class TierManager
{
    // 명세서에 따른 영문 명칭
    private static readonly string[] Tiers =
    {
        "Bronze",
        "Silver",
        "Gold",
        "Platinum",
        "Diamond",
        "Master",
        "Challenger",
    };
    private static readonly string[] Grades =
    {
        "Normal",
        "Core",
        "Magic",
        "Rare",
        "Elite",
        "Unique",
        "Legend",
    };

    public struct TierInfo
    {
        public string fullName; // "Normal Bronze"
        public int tierIdx; // 0 ~ 6
        public int gradeIdx; // 0 ~ 6
        public int spriteIdx; // 0 ~ 48 (이미지가 49개일 경우)
    }

    public static TierInfo GetTierInfo(int totalAR)
    {
        // 점수가 음수일 경우를 대비한 클램프
        int ar = Mathf.Max(0, totalAR);

        int tIdx = Mathf.Min(ar / 700, Tiers.Length - 1); // 티어 (0~6)
        int gIdx = Mathf.Min((ar % 700) / 100, Grades.Length - 1); // 단계 (0~6)

        return new TierInfo
        {
            fullName = $"{Grades[gIdx]} {Tiers[tIdx]}",
            tierIdx = tIdx,
            gradeIdx = gIdx,
            spriteIdx = (tIdx * 7) + gIdx, // 49개 이미지 매핑용 인덱스
        };
    }
}
