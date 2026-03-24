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
    private TMP_Text titleText; // 왼쪽: "훈련장-개념이해"

    [SerializeField]
    private TMP_Text timerText; // 가운데: "5:00"

    [Header("Exit Scene")]
    [SerializeField]
    private string exitSceneName = "05_ExperienceSelect"; // 훈련은 "07_TrainingSelect" 등

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
        ResetTimer(); // 시작 시 타이머 초기화
        currentCategory = ReadCurrentCategory();
        SetupTitle();
    }

    public void ResetTimer()
    {
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

            // 시간 초과 시 오답 처리 및 다음 로직 호출 (컨트롤러와 연결 필요)
            if (mode != BattleMode.Experience)
            {
                Object.FindFirstObjectByType<ExperienceBattleController>().OnTimeOut();
            }
        }
        UpdateTimerText();
    }

    private CommonCategory ReadCurrentCategory()
    {
        switch (mode)
        {
            case BattleMode.Training:
                return TrainingSession.CurrentCategory switch
                {
                    TrainingCategory.Concept => CommonCategory.Concept,
                    TrainingCategory.Calc => CommonCategory.Calc,
                    TrainingCategory.Idea => CommonCategory.Idea,
                    TrainingCategory.Design => CommonCategory.Design,
                    TrainingCategory.Practice => CommonCategory.Practice,
                    _ => CommonCategory.Concept,
                };

            case BattleMode.Arena:
                // 아레나가 어떤 세션을 쓸지 확정되면 맞춰서 수정
                return ExperienceSession.CurrentCategory switch
                {
                    ExperienceCategory.Concept => CommonCategory.Concept,
                    ExperienceCategory.Calc => CommonCategory.Calc,
                    ExperienceCategory.Idea => CommonCategory.Idea,
                    ExperienceCategory.Design => CommonCategory.Design,
                    ExperienceCategory.Practice => CommonCategory.Practice,
                    _ => CommonCategory.Concept,
                };

            case BattleMode.Experience:
            default:
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

        titleText.text = $"{prefix}-{categoryKorean}";
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        // 전체 시간을 초 단위로 올림 처리합니다. (예: 59.1초 -> 60초)
        int totalSeconds = Mathf.CeilToInt(remainingTime);

        // "60", "59" 처럼 숫자만 나오게 하거나 뒤에 "초"를 붙일 수 있습니다.
        timerText.text = totalSeconds.ToString();

        // 만약 "60초"라고 표시하고 싶다면 아래 코드를 사용하세요.
        // timerText.text = $"{totalSeconds}초";
    }

    public void OnClickExit()
    {
        isRunning = false;

        if (string.IsNullOrEmpty(exitSceneName))
            exitSceneName = "02_Lobby";

        SceneManager.LoadScene(exitSceneName);
    }
}
