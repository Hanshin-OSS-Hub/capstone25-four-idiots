using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrainingResultUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField]
    private GameObject rootPanel;

    [Header("Result Texts")]
    [SerializeField]
    private TMP_Text bpScoreText; // "200 BP"

    [SerializeField]
    private TMP_Text updateStatusText; // "전투력이 업데이트 되었습니다"

    [SerializeField]
    private TMP_Text unlockMessageText; // "전투력 200을 달성하여..."

    /// <summary>
    /// 훈련 결과를 화면에 표시합니다.
    /// </summary>
    /// <param name="finalBP">최종 계산된 BP</param>
    /// <param name="unlockedDiffs">새로 해금된 난이도 이름 (예: "HARD, VERY HARD")</param>
    public void Show(int finalBP, string unlockedDiffs = "")
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        if (bpScoreText != null)
            bpScoreText.text = $"{finalBP} BP";

        // [수정] 해금 메시지 텍스트를 아예 비활성화하여 화면에 뜨지 않게 합니다.
        if (unlockMessageText != null)
        {
            unlockMessageText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 나가기 버튼 클릭 시 호출 (훈련 종목 선택 화면으로 이동)
    /// </summary>
    public void OnClickExit()
    {
        // 07_TrainingSelect 씬으로 이동 (기존 세션 설정에 맞춤)
        SceneManager.LoadScene("07_TrainingSelect");
    }
}
