using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyProfileController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text textNickname;
    public TMP_Text textAverageCP;
    public TMP_Text textTierName;
    public Image imgTierIcon;
    public TMP_Text textGold;
    public TMP_Text textTicket;

    private void Start()
    {
        // 씬이 시작될 때 서버에서 내 정보를 가져옴
        RefreshUserData();
    }

    public void RefreshUserData()
    {
        if (NetworkManager.Instance == null)
            return;

        NetworkManager.Instance.GetProfile(
            onSuccess: (data) => UpdateUI(data),
            onFail: (error) => Debug.LogError($"프로필 로드 실패: {error}")
        );
    }

    private void UpdateUI(UserProfileData data)
    {
        textNickname.text = data.nickname;

        // 평균 전투력 계산 (전 종목 합산 평균)
        float avg =
            (data.cp_concept + data.cp_calc + data.cp_idea + data.cp_design + data.cp_practical)
            / 5f;
        textAverageCP.text = $"평균전투력 {Mathf.RoundToInt(avg)}";

        // 껍데기 정보
        textGold.text = data.gold.ToString();
        textTicket.text = data.arenaTickets.ToString();

        // 티어 및 이미지 로드 (Resources/Tiers/Tier_X_Y)
        var info = TierManager.GetTierInfo(data.arena_rating);
        textTierName.text = info.fullName;

        string path = $"Tiers/Tier_{info.tierIdx}_{info.gradeIdx}";
        imgTierIcon.sprite = Resources.Load<Sprite>(path);
    }
}
