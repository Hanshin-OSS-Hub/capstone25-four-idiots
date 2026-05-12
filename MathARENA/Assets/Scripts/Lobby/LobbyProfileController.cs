using MathArena.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyProfileController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private TMP_Text textNickname;

    [SerializeField]
    private TMP_Text textAverageCP; // "전투력 2840" 형태 [cite: 18]

    [SerializeField]
    private TMP_Text textTierName; // "Normal Bronze" 형태 [cite: 19]

    [SerializeField]
    private Image imgTierIcon; // 49단계 티어 아이콘 [cite: 19, 46, 47]

    [SerializeField]
    private TMP_Text textGold;

    [SerializeField]
    private TMP_Text textTicket;

    [Header("Category CP Texts")]
    [SerializeField]
    private TMP_Text textConceptCP;

    [SerializeField]
    private TMP_Text textCalcCP;

    [SerializeField]
    private TMP_Text textIdeaCP;

    [SerializeField]
    private TMP_Text textDesignCP;

    [SerializeField]
    private TMP_Text textPracticalCP;

    private void Start()
    {
        RefreshUserData();
    }

    public void RefreshUserData()
    {
        if (NetworkManager.Instance == null)
            return;

        NetworkManager.Instance.GetProfile(
            onSuccess: (data) => UpdateUI(data),
            onFail: (error) => Debug.LogError($"[Profile] 로드 실패: {error}")
        );
    }

    private void UpdateUI(UserProfileData data)
    {
        if (textNickname != null)
            textNickname.text = data.nickname;

        float avg =
            (data.cp_concept + data.cp_calc + data.cp_idea + data.cp_design + data.cp_practical)
            / 5f;
        if (textAverageCP != null)
            textAverageCP.text = $"평균전투력 {Mathf.RoundToInt(avg)}";

        var info = TierManager.GetTierInfo(data.arena_rating);
        if (textTierName != null)
            textTierName.text = info.fullName;

        // 3. 종목별 개별 전투력 표시
        if (textConceptCP != null)
            textConceptCP.text = $"{data.cp_concept} BP";
        if (textCalcCP != null)
            textCalcCP.text = $"{data.cp_calc} BP";
        if (textIdeaCP != null)
            textIdeaCP.text = $"{data.cp_idea} BP";
        if (textDesignCP != null)
            textDesignCP.text = $"{data.cp_design} BP";
        if (textPracticalCP != null)
            textPracticalCP.text = $"{data.cp_practical} BP";

        // 4. 재화 정보
        if (textGold != null)
            textGold.text = $"X {data.gold}";
        if (textTicket != null)
            textTicket.text = $"X {data.arenaTickets}";

        string path = $"Tiers/Tier_{info.tierIdx}_{info.gradeIdx}";
        if (imgTierIcon != null)
            imgTierIcon.sprite = Resources.Load<Sprite>(path);
    }
}