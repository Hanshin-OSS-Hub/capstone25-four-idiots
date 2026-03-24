using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBattleController : MonoBehaviour
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

    [Header("Panels")]
    [SerializeField]
    private GameObject panelChoices;

    [SerializeField]
    private GameObject panelInput;

    [Header("Question")]
    [SerializeField]
    private TMP_Text questionText;

    [Header("Choices (4지선다)")]
    [SerializeField]
    private Button[] choiceButtons = new Button[4];

    [SerializeField]
    private TMP_Text[] choiceTexts = new TMP_Text[4];

    [Header("Input Mode")]
    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private Button submitButton;

    [Header("UI References")]
    [SerializeField]
    private ExperienceDifficultyBar difficultyBar;

    [SerializeField]
    private ExperienceResultUI resultUI;

    [Header("Life UI")]
    [SerializeField]
    private TMP_Text lifeText;

    [SerializeField]
    private Image[] heartIcons = new Image[4];

    [SerializeField]
    private Sprite activeHeartSprite;

    [SerializeField]
    private Sprite inactiveHeartSprite;

    [Header("OCR System")]
    [SerializeField]
    private SentisOCRManager ocrManager;

    [SerializeField]
    private DrawingCanvas drawingCanvas;

    [Header("OCR UI References")]
    [SerializeField]
    private TMP_Text verifiedDigitText; // 왼쪽 위 "입력된 숫자 : ?" 텍스트
    private int lastPredictedValue = -1; // 검증된 숫자를 임시 저장

    private CommonCategory currentCategory;
    private int correctChoiceIndex = 0;
    private string correctAnswerText = "";
    private int correctCount = 0;

    private enum CommonCategory
    {
        Concept,
        Calc,
        Idea,
        Design,
        Practice,
    }

    private void Awake()
    {
        ExperienceSession.CurrentLife = ExperienceSession.MaxLife;
        UpdateLifeUI();
        ExperienceSession.TotalExpScore = 0;
        ExperienceSession.CurrentQuestionCount = 0;

        currentCategory = ReadCurrentCategory();
        SetupModeByCategory();
        SetupQuestionByCategory();
        HookChoiceButtons();
        HookSubmitButton();
    }

    public void OnTimeOut() => ProcessAnswer(false);

    private CommonCategory ReadCurrentCategory()
    {
        if (mode == BattleMode.Training)
        {
            return TrainingSession.CurrentCategory switch
            {
                TrainingCategory.Concept => CommonCategory.Concept,
                TrainingCategory.Calc => CommonCategory.Calc,
                TrainingCategory.Idea => CommonCategory.Idea,
                TrainingCategory.Design => CommonCategory.Design,
                TrainingCategory.Practice => CommonCategory.Practice,
                _ => CommonCategory.Concept,
            };
        }

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

    private void SetupModeByCategory()
    {
        if (panelChoices != null)
            panelChoices.SetActive(
                currentCategory == CommonCategory.Concept || currentCategory == CommonCategory.Idea
            );
        if (panelInput != null)
            panelInput.SetActive(
                currentCategory == CommonCategory.Calc || currentCategory == CommonCategory.Practice
            );
    }

    private void SetupQuestionByCategory()
    {
        switch (currentCategory)
        {
            case CommonCategory.Concept:
                SetupConceptQuestion();
                break;
            case CommonCategory.Idea:
                SetupIdeaQuestion();
                break;
            case CommonCategory.Calc:
                SetupCalcQuestion();
                break;
            case CommonCategory.Practice:
                SetupPracticeQuestion();
                break;
            default:
                SetupDesignQuestion();
                break;
        }
    }

    // --- 문제 세팅 함수들 (생략) ---
    private void SetupConceptQuestion() { /* 기존 내용 유지 */
    }

    private void SetupIdeaQuestion() { /* 기존 내용 유지 */
    }

    private void SetupCalcQuestion()
    {
        if (questionText != null)
            questionText.text = "다음 연산의 결과는?\n(12 ÷ 4) + 2 = ?";

        correctAnswerText = "5"; // 정답: 5
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.VeryEasy);
    }

    private void SetupPracticeQuestion()
    {
        if (questionText != null)
            questionText.text = "방정식 2x - 6 = 10 이라면,\nx - 1 의 값은?";

        // 2x = 16 -> x = 8 -> x - 1 = 7
        correctAnswerText = "7"; // 정답: 7
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.Easy);
    }

    private void SetupDesignQuestion()
    {
        if (questionText != null)
            questionText.text = "함수 f(x) = 3x - 9 가 있을 때,\nf(3)의 값은 얼마인가?";

        correctAnswerText = "0"; // 정답: 0

        // [수정] ExperienceDifficulty.Normal -> .Easy 또는 .Hard로 변경
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.Easy);
    }

    private void OnClickChoice(int index) => ProcessAnswer(index == correctChoiceIndex);

    // [1] 검증 버튼 전용 함수 (새로 만든 버튼에 연결)
    public void OnClickVerifyDigit()
    {
        if (drawingCanvas == null || ocrManager == null)
            return;

        Texture2D captured = drawingCanvas.GetCapturedTexture();
        lastPredictedValue = ocrManager.PredictDigit(captured);

        if (verifiedDigitText != null)
            verifiedDigitText.text = $"입력된 숫자 : {lastPredictedValue}";
    }

    // [2] 지우기 버튼 전용 함수 (새로 만든 버튼에 연결)
    public void OnClickClearCanvas()
    {
        drawingCanvas.ClearCanvas();
        lastPredictedValue = -1;
        if (verifiedDigitText != null)
            verifiedDigitText.text = "입력된 숫자 : ";
    }

    // [3] 통합된 답안 제출 함수 (기존 제출 버튼에 연결)
    public void OnClickSubmitAnswer()
    {
        if (lastPredictedValue == -1)
        {
            Debug.LogWarning("먼저 숫자를 검증해주세요!");
            return;
        }

        bool isCorrect = (lastPredictedValue.ToString() == correctAnswerText);
        ProcessAnswer(isCorrect);
        OnClickClearCanvas(); // 제출 후 자동 초기화
    }

    private void ProcessAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            correctCount++;
            ExperienceSession.TotalExpScore += (int)ExperienceSession.CurrentDifficulty * 10;
        }
        else if (mode != BattleMode.Experience)
        {
            ExperienceSession.CurrentLife--;
            UpdateLifeUI();
        }

        ExperienceSession.CurrentQuestionCount++;

        if (mode != BattleMode.Experience && ExperienceSession.CurrentLife <= 0)
            FinishExperience();
        else if (mode == BattleMode.Experience)
            FinishExperience();
        else
        {
            Object.FindFirstObjectByType<ExperienceBattleAppBar>().ResetTimer();
            SetupQuestionByCategory();
        }
    }

    public void FinishExperience()
    {
        if (resultUI != null)
            resultUI.Show(
                correctCount,
                ExperienceSession.CurrentQuestionCount,
                ExperienceSession.TotalExpScore
            );
    }

    private void UpdateLifeUI()
    {
        if (lifeText != null)
            lifeText.text = $"Life: {ExperienceSession.CurrentLife}";
        if (heartIcons != null && heartIcons.Length == 4)
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (activeHeartSprite != null && inactiveHeartSprite != null)
                    heartIcons[i].sprite =
                        (i < ExperienceSession.CurrentLife)
                            ? activeHeartSprite
                            : inactiveHeartSprite;
            }
        }
    }

    private void SetChoiceText(int index, string text)
    {
        if (index >= 0 && index < choiceTexts.Length && choiceTexts[index] != null)
            choiceTexts[index].text = text;
    }

    private void HookChoiceButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int idx = i;
            choiceButtons[i]?.onClick.RemoveAllListeners();
            choiceButtons[i]?.onClick.AddListener(() => OnClickChoice(idx));
        }
    }

    private void HookSubmitButton()
    {
        if (submitButton == null)
            return;
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(OnClickSubmitAnswer);
    }
}
