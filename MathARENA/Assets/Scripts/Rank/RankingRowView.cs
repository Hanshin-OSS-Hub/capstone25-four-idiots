using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingRowView : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite bronzeBackground;
    [SerializeField] private Sprite silverBackground;
    [SerializeField] private Sprite goldBackground;

    [Header("Rank Icon")]
    [SerializeField] private Image rankImage;
    [SerializeField] private Sprite bronzeRankSprite;
    [SerializeField] private Sprite silverRankSprite;
    [SerializeField] private Sprite goldRankSprite;

    [Header("Texts / Images")]
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private Image profileIconImage;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text arText;

    public void Setup(RankingEntryData data)
    {
        if (rankText != null)
            rankText.text = data.rank.ToString();

        if (profileIconImage != null)
            profileIconImage.sprite = data.profileIcon;

        if (nicknameText != null)
            nicknameText.text = data.nickname;

        if (levelText != null)
            levelText.text = $"Lv.{data.level}";

        if (scoreText != null)
            scoreText.text = $"{data.score} BP";

        if (arText != null)
            arText.text = $"{data.ar} AR";

        ApplyTierVisual(data.tier);
    }

    private void ApplyTierVisual(RankingTierType tier)
    {
        ApplyBackground(tier);
        ApplyRankIcon(tier);
    }

    private void ApplyBackground(RankingTierType tier)
    {
        if (backgroundImage == null) return;

        switch (tier)
        {
            case RankingTierType.Gold:
                backgroundImage.sprite = goldBackground;
                break;

            case RankingTierType.Silver:
                backgroundImage.sprite = silverBackground;
                break;

            default:
                backgroundImage.sprite = bronzeBackground;
                break;
        }
    }

    private void ApplyRankIcon(RankingTierType tier)
    {
        if (rankImage == null) return;

        switch (tier)
        {
            case RankingTierType.Gold:
                rankImage.sprite = goldRankSprite;
                break;

            case RankingTierType.Silver:
                rankImage.sprite = silverRankSprite;
                break;

            default:
                rankImage.sprite = bronzeRankSprite;
                break;
        }
    }
}