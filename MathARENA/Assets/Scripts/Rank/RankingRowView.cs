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
        arText.text = $"{data.ar} AR";

        // 49단계 티어 이미지 자동 로드
        var info = TierManager.GetTierInfo(data.ar);
        string path = $"Tiers/Tier_{info.tierIdx}_{info.gradeIdx}";
        tierIconImage.sprite = Resources.Load<Sprite>(path);
    }
}
