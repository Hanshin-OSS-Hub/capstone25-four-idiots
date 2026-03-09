using System;
using UnityEngine;

[Serializable]
public class RankingEntryData
{
    public int rank;
    public string nickname;
    public int level;
    public int score;
    public int ar;
    public Sprite profileIcon;
    public RankingTierType tier;
}