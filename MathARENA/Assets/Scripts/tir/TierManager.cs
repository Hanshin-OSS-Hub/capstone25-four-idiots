using UnityEngine;

public static class TierManager
{
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
        public string fullName;
        public int tierIdx;
        public int gradeIdx;
    }

    public static TierInfo GetTierInfo(int ar)
    {
        int totalAR = Mathf.Max(0, ar);
        int totalSteps = totalAR / 100;
        int tIdx = Mathf.Min(totalSteps / 7, Tiers.Length - 1);
        int gIdx = Mathf.Min(totalSteps % 7, Grades.Length - 1);

        return new TierInfo
        {
            fullName = $"{Grades[gIdx]} {Tiers[tIdx]}",
            tierIdx = tIdx,
            gradeIdx = gIdx,
        };
    }
}
