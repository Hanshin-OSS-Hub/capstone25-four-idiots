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
    private TMP_Text resultText;

    [Header("Rating Changes")]
    [SerializeField]
    private TMP_Text arChangeText;

    [SerializeField]
    private TMP_Text arProgressText;

    [Header("Tier Status")]
    [SerializeField]
    private TMP_Text tierNameText;

    [SerializeField]
    private Image tierIconImage;

    [Header("Promotion Alert")]
    [SerializeField]
    private TMP_Text promotionText; // [추가] "티어 승급!" 알림 텍스트

    public void Show(
        bool isWin,
        int prevAr,
        int currentAr,
        int changeAmount,
        string tierName,
        Sprite tierSprite,
        bool isPromoted // [추가] 승급 여부 매개변수 [cite: 49]
    )
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text = isWin ? "승리" : "패배";
            resultText.color = isWin ? new Color(0.1f, 0.8f, 0.4f) : Color.red;
        }

        if (arChangeText != null)
        {
            string sign = changeAmount >= 0 ? "+" : "";
            arChangeText.text = $"{sign}{changeAmount} AR";
            arChangeText.color = changeAmount >= 0 ? new Color(0.1f, 0.8f, 0.4f) : Color.red;
        }

        if (arProgressText != null)
        {
            arProgressText.text = $"{prevAr} AR  →  {currentAr} AR";
        }

        if (tierNameText != null)
            tierNameText.text = tierName;
        if (tierIconImage != null)
            tierIconImage.sprite = tierSprite;

        if (promotionText != null)
        {
            promotionText.gameObject.SetActive(isPromoted);
            if (isPromoted)
                promotionText.text = "티어 승급!";
        }
    }

    public void OnClickExit()
    {
        SceneManager.LoadScene("02_Lobby");
    }
}
