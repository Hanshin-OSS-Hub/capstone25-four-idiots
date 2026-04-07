using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyProfileController : MonoBehaviour
{
    [Header("UI References (Center_Tier)")]
    [SerializeField]
    private Image imgTierIcon;

    [SerializeField]
    private TMP_Text textTierName;

    [SerializeField]
    private TMP_Text textBattlePower;

    [Header("UI References (Right_Stats)")]
    [SerializeField]
    private TMP_Text textGold;

    [SerializeField]
    private TMP_Text textTicket;

    [Header("Detailed Profile Panel")]
    [SerializeField]
    private GameObject detailedProfilePanel;

    private void Start()
    {
        // 씬 시작 시 서버 데이터 로드 (추후 NetworkManager와 연동)
        RefreshProfile();
    }

    public void RefreshProfile()
    {
        // [서버 연동부] 현재는 명세서 초기값(Normal Bronze, 0AR, 0BP) 기준 더미 데이터
        UserProfileData dummyData = new UserProfileData
        {
            totalAR = 0,
            averageBP = 0,
            gold = 0,
            arenaTickets = 0,
        };

        UpdateUI(dummyData);
    }

    private void UpdateUI(UserProfileData data)
    {
        // 1. 수학적 계산을 통해 티어(0~6)와 단계(0~6) 인덱스 추출
        int ar = Mathf.Max(0, data.totalAR);
        int tierIdx = Mathf.Min(ar / 700, 6); // 700점마다 티어 상승
        int gradeIdx = Mathf.Min((ar % 700) / 100, 6); // 100점마다 단계 상승

        // 2. 티어 명칭 설정 (영문 표기)
        textTierName.text = GetTierFullName(tierIdx, gradeIdx);

        // 3. 리소스 자동 로드 (Tier_T_G 규칙 사용)
        // 경로: Assets/Resources/Tiers/Tier_0_0.png 등
        string spritePath = $"Tiers/Tier_{tierIdx}_{gradeIdx}";
        Sprite loadedTierSprite = Resources.Load<Sprite>(spritePath);

        if (loadedTierSprite != null)
            imgTierIcon.sprite = loadedTierSprite;
        else
            Debug.LogWarning($"[LobbyProfile] 리소스를 찾을 수 없습니다: {spritePath}");

        // 4. 전투력 및 재화 업데이트 (명세 준수)
        textBattlePower.text = $"Average BP: {data.averageBP}";
        textGold.text = $"x{data.gold}";
        textTicket.text = $"x{data.arenaTickets}";
    }

    private string GetTierFullName(int tIdx, int gIdx)
    {
        string[] tiers =
        {
            "Bronze",
            "Silver",
            "Gold",
            "Platinum",
            "Diamond",
            "Master",
            "Challenger",
        };
        string[] grades = { "Normal", "Core", "Magic", "Rare", "Elite", "Unique", "Legend" };

        // 결과 예: "Normal Bronze"
        return $"{grades[gIdx]} {tiers[tIdx]}";
    }

    // Panel_TopProfile의 Button OnClick에 연결
    public void OnClickProfileBox()
    {
        if (detailedProfilePanel != null)
            detailedProfilePanel.SetActive(true);
    }
}
