using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ExperienceBattleAppBar : MonoBehaviour
{
    public enum BattleMode
    {
        Experience,
        Training,
        Arena,
    }

    [Header("Mode")]
    [SerializeField]
    private BattleMode mode = BattleMode.Experience;

    [Header("UI References")]
    [SerializeField]
    private TMP_Text titleText; // 왼쪽 제목: "아레나-설계" 등

    // [중요] timerText는 이제 ExperienceTimer에서 직접 제어하므로 여기서 뺍니다.

    [Header("Exit Scene")]
    [SerializeField]
    private string exitSceneName = "02_Lobby";

    private enum CommonCategory
    {
        Concept,
        Calc,
        Idea,
        Design,
        Practice,
    }

    private CommonCategory currentCategory;

    private void Start()
    {
        currentCategory = ReadCurrentCategory();
        SetupTitle();
    }

    // [삭제] ResetTimer(), Update(), UpdateTimerText()를 모두 제거했습니다.

    private CommonCategory ReadCurrentCategory()
    {
        // [수정 완료] 훈련장과 체험장의 세션 데이터가 통합되어 있다면 동일하게 ExperienceSession을 참조하도록 수정합니다.
        // 만약 아레나도 통합 세션을 쓴다면 전원 ExperienceSession으로 대체해도 좋습니다.
        return mode switch
        {
            BattleMode.Arena => (CommonCategory)ArenaSession.CurrentCategory,
            BattleMode.Training => (CommonCategory)ExperienceSession.CurrentCategory, // 훈련장도 ExperienceSession의 카테고리를 읽도록 변경
            _ => (CommonCategory)ExperienceSession.CurrentCategory,
        };
    }

    private void SetupTitle()
    {
        if (titleText == null)
            return;
        string prefix =
            mode == BattleMode.Training ? "훈련장"
            : mode == BattleMode.Arena ? "아레나"
            : "체험장";
        string catKwn = currentCategory switch
        {
            CommonCategory.Concept => "개념이해",
            CommonCategory.Calc => "연산",
            CommonCategory.Idea => "발상",
            CommonCategory.Design => "설계",
            _ => "실전",
        };
        titleText.text = $"{prefix}-{catKwn}";
    }

    public void OnClickExit()
    {
        SceneManager.LoadScene(string.IsNullOrEmpty(exitSceneName) ? "02_Lobby" : exitSceneName);
    }
}
