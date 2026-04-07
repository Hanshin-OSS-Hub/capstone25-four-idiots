using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyProfileManager : MonoBehaviour
{
    [Header("Center_Tier References")]
    [SerializeField]
    private Image imgTierIcon;

    [SerializeField]
    private TMP_Text textTierName;

    [SerializeField]
    private TMP_Text textBattlePower;

    [Header("Right_Stats References")]
    [SerializeField]
    private TMP_Text textGold;

    [SerializeField]
    private TMP_Text textTicket;

    [Header("Tier Sprites (0:Iron ~ 6:Master)")]
    [SerializeField]
    private Sprite[] tierSprites;

    // 상세 프로필 이동 시 사용할 패널 (인스펙터에서 연결)
    [SerializeField]
    private GameObject detailedProfilePanel;

    private void Start()
    {
        // 씬 시작 시 서버 데이터 로드 시뮬레이션
        RefreshProfile();
    }

    // 서버로부터 최신 정보를 가져와 UI를 갱신하는 함수
    public void RefreshProfile()
    {
        // [서버 연동부] 나중에 실제 NetworkManager.Instance.GetUserData() 등으로 대체
        // 지금은 기획 명세에 맞춘 더미 데이터 적용
        int currentRating = 450; // 브론즈 구간 예시
        int avgBP = 0; // 명세: 수치는 0으로 고정
        int gold = 0; // 명세: x0으로 고정
        int tickets = 0; // 명세: x0으로 고정

        UpdateProfileUI(currentRating, avgBP, gold, tickets);
    }

    private void UpdateProfileUI(int score, int bp, int gold, int ticket)
    {
        // 1. 티어 인덱스 계산 (400점 단위 가정)
        int tierIdx = Mathf.Clamp(score / 400, 0, tierSprites.Length - 1);

        // 2. 티어 텍스트 적용 (명세: 영어로 표기)
        textTierName.text = GetTierEnglishName(tierIdx);
        imgTierIcon.sprite = tierSprites[tierIdx];

        // 3. 전투력 및 재화 적용
        textBattlePower.text = $"Average BP: {bp}";
        textGold.text = $"x{gold}";
        textTicket.text = $"x{ticket}";
    }

    private string GetTierEnglishName(int idx)
    {
        string[] names =
        {
            "Normal Iron",
            "Normal Bronze",
            "Normal Silver",
            "Normal Gold",
            "Normal Platinum",
            "Normal Diamond",
            "Normal Master",
        };
        return names[idx];
    }

    // Panel_TopProfile의 Button OnClick에 연결할 함수
    public void OnClickProfileBox()
    {
        Debug.Log("프로필 박스 클릭됨 -> 상세 정보 오픈");
        if (detailedProfilePanel != null)
            detailedProfilePanel.SetActive(true);
    }
}
