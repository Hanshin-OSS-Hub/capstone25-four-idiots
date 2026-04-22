using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUIController : MonoBehaviour
{
    [Header("Top Row: 3 Boxes")]
    [SerializeField]
    private Image imgIllustration;

    [SerializeField]
    private TMP_Text textNickname;

    [SerializeField]
    private Image imgTierIcon;

    [SerializeField]
    private TMP_Text textTierName;

    [SerializeField]
    private TMP_Text textArenaRating;

    [Header("Bottom Row: 5 CP Boxes")]
    [SerializeField]
    private TMP_Text[] cpValueTexts;

    [Header("Currency")]
    [SerializeField]
    private TMP_Text textGold;

    [SerializeField]
    private TMP_Text textTicket;

    public void UpdateProfileUI(UserProfileData data)
    {
        textNickname.text = data.nickname;

        var tierInfo = TierManager.GetTierInfo(data.arena_rating);
        textTierName.text = tierInfo.fullName;
        imgTierIcon.sprite = Resources.Load<Sprite>(
            $"Tiers/Tier_{tierInfo.tierIdx}_{tierInfo.gradeIdx}"
        );

        textArenaRating.text = $"{data.arena_rating} AR";

        if (cpValueTexts.Length >= 5)
        {
            cpValueTexts[0].text = $"{data.cp_concept} BP";
            cpValueTexts[1].text = $"{data.cp_calc} BP";
            cpValueTexts[2].text = $"{data.cp_idea} BP";
            cpValueTexts[3].text = $"{data.cp_design} BP";
            cpValueTexts[4].text = $"{data.cp_practical} BP";
        }

        textGold.text = data.gold.ToString();
        textTicket.text = data.arenaTickets.ToString();
    }
}
