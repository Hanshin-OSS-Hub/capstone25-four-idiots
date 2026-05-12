using MathArena.Network; // [추가] 네임스페이스 연결
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingRowView : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text nicknameText;
    public TMP_Text arText;
    public Image tierIconImage;

    public void Setup(RankingEntryData data)
    {
        rankText.text = data.rank.ToString();
        nicknameText.text = data.nickname;

        // [수정] ar 대신 arena_rating 사용
        arText.text = $"{data.arena_rating} AR";

        var info = TierManager.GetTierInfo(data.arena_rating);
        string path = $"Tiers/Tier_{info.tierIdx}_{info.gradeIdx}";
        tierIconImage.sprite = Resources.Load<Sprite>(path);
    }
}
