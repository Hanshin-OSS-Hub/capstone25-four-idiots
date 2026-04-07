using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArenaResultUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField]
    private GameObject rootPanel;

    [Header("Result Header")]
    [SerializeField]
    private TMP_Text resultText; // "승리" 또는 "패배"

    [Header("Rating Changes")]
    [SerializeField]
    private TMP_Text arChangeText; // "+25 AR"

    [SerializeField]
    private TMP_Text arProgressText; // "1247 AR -> 1272 AR"

    [Header("Tier Status")]
    [SerializeField]
    private TMP_Text tierNameText; // "용사" 또는 "레전드 브론즈"

    [SerializeField]
    private Image tierIconImage; // 티어 단계별 아이콘

    /// <summary>
    /// 아레나 배틀 종료 시 결과를 UI에 세팅합니다.
    /// </summary>
    public void Show(
        bool isWin,
        int prevAr,
        int currentAr,
        int changeAmount,
        string tierName,
        Sprite tierSprite
    )
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        // 1. 승패 결과
        if (resultText != null)
        {
            resultText.text = isWin ? "승리" : "패배";
            resultText.color = isWin ? new Color(0.1f, 0.8f, 0.4f) : Color.red; // 승리 시 녹색 계열
        }

        // 2. 획득 또는 차감된 아레나 레이팅
        if (arChangeText != null)
        {
            string sign = changeAmount >= 0 ? "+" : "";
            arChangeText.text = $"{sign}{changeAmount} AR";
            arChangeText.color = changeAmount >= 0 ? new Color(0.1f, 0.8f, 0.4f) : Color.red;
        }

        // 3. 아레나 레이팅 현황 (Before -> After)
        if (arProgressText != null)
        {
            arProgressText.text = $"{prevAr} AR  →  {currentAr} AR";
        }

        // 4. 티어 및 단계 현황 보고
        if (tierNameText != null)
            tierNameText.text = tierName;
        if (tierIconImage != null)
            tierIconImage.sprite = tierSprite;
    }

    /// <summary>
    /// 나가기 버튼 클릭 시 호출
    /// </summary>
    public void OnClickExit()
    {
        // 명세서에 따라 아레나 종목 선택 화면으로 이동
        SceneManager.LoadScene("02_Lobby");
    }
}
