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
    private TMP_Text titleText; // 왼쪽: "아레나-설계" 등

    [SerializeField]
    private TMP_Text timerText; // 가운데: "60"

    [Header("Exit Scene")]
    [SerializeField]
    private string exitSceneName = "02_Lobby";

    [Header("Timer Settings")]
    [SerializeField]
    private int startMinutes = 5;

    private float remainingTime;
    private bool isRunning = true;

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
        ResetTimer();
        currentCategory = ReadCurrentCategory();
        SetupTitle();
    }

    public void ResetTimer()
    {
        // 명세서에 따라 체험장은 설정된 시간, 그 외는 세션의 제한시간을 따릅니다.
        remainingTime =
            (mode == BattleMode.Experience)
                ? startMinutes * 60f
                : ExperienceSession.QuestionTimeLimit;
        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;

            // 시간 초과 시 오답 처리
            if (mode != BattleMode.Experience)
            {
                Object.FindFirstObjectByType<ExperienceBattleController>()?.OnTimeOut();
            }
        }
        UpdateTimerText();
    }

    // [버그 수정 포인트] 아레나 모드일 때 ArenaSession을 정확히 참조하도록 수정했습니다.
    private CommonCategory ReadCurrentCategory()
    {
        switch (mode)
        {
            case BattleMode.Arena:
                // 아레나 세션 데이터 로드
                return ArenaSession.CurrentCategory switch
                {
                    ArenaCategory.Concept => CommonCategory.Concept,
                    ArenaCategory.Calc => CommonCategory.Calc,
                    ArenaCategory.Idea => CommonCategory.Idea,
                    ArenaCategory.Design => CommonCategory.Design,
                    ArenaCategory.Practice => CommonCategory.Practice,
                    _ => CommonCategory.Concept,
                };

            case BattleMode.Training:
                // 훈련장 세션 데이터 로드
                return TrainingSession.CurrentCategory switch
                {
                    TrainingCategory.Concept => CommonCategory.Concept,
                    TrainingCategory.Calc => CommonCategory.Calc,
                    TrainingCategory.Idea => CommonCategory.Idea,
                    TrainingCategory.Design => CommonCategory.Design,
                    TrainingCategory.Practice => CommonCategory.Practice,
                    _ => CommonCategory.Concept,
                };

            case BattleMode.Experience:
            default:
                // 체험장 세션 데이터 로드
                return ExperienceSession.CurrentCategory switch
                {
                    ExperienceCategory.Concept => CommonCategory.Concept,
                    ExperienceCategory.Calc => CommonCategory.Calc,
                    ExperienceCategory.Idea => CommonCategory.Idea,
                    ExperienceCategory.Design => CommonCategory.Design,
                    ExperienceCategory.Practice => CommonCategory.Practice,
                    _ => CommonCategory.Concept,
                };
        }
    }

    private void SetupTitle()
    {
        if (titleText == null)
            return;

        string prefix = mode switch
        {
            BattleMode.Training => "훈련장",
            BattleMode.Arena => "아레나",
            _ => "체험장",
        };

        string categoryKorean = currentCategory switch
        {
            CommonCategory.Concept => "개념이해",
            CommonCategory.Calc => "연산",
            CommonCategory.Idea => "발상",
            CommonCategory.Design => "설계",
            CommonCategory.Practice => "실전",
            _ => "개념이해",
        };

        // UI에 최종 텍스트 적용
        titleText.text = $"{prefix}-{categoryKorean}";
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(remainingTime);
        timerText.text = totalSeconds.ToString();
    }

    public void OnClickExit()
    {
        isRunning = false;

        // 명세서 상의 퇴장 씬 설정
        if (string.IsNullOrEmpty(exitSceneName))
            exitSceneName = "02_Lobby";

        SceneManager.LoadScene(exitSceneName);
    }
}
