using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExperienceResultUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField]
    private GameObject rootPanel; // 결과창 최상위 패널

    [Header("Result Texts")]
    [SerializeField]
    private TMP_Text correctCountText; // "맞힌 문제 개수 1 / 1"

    [SerializeField]
    private TMP_Text battlePowerText; // "체험 전투력: 100 BP"

    /// <summary>
    /// 결과 화면을 활성화하고 데이터를 표시합니다.
    /// </summary>
    public void Show(int correctCount, int totalCount, int totalScore)
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        // 맞힌 문제 개수 텍스트 업데이트
        if (correctCountText != null)
        {
            correctCountText.text =
                $"맞힌 문제 개수\n{correctCount} / {totalCount}";
        }

        // 체험 전투력 보고 (BP 표시)
        if (battlePowerText != null)
        {
            battlePowerText.text = $"체험 전투력: {totalScore:N0} BP";
        }
    }

    /// <summary>
    /// 확인 버튼 클릭 시 호출 (선택 화면으로 이동)
    /// </summary>
    public void OnClickConfirm()
    {
        // 05_ExperienceSelect 씬으로 이동
        SceneManager.LoadScene("05_ExperienceSelect");
    }
}
