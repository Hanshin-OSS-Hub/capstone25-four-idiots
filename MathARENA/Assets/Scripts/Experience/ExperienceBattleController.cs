using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private GameObject panelDesign;

    [SerializeField]
    private GameObject panelInput;

    [Header("Question Counter UI")]
    [SerializeField]
    private TMP_Text questionCounterText; // "1 / 30" 텍스트 연결

    [SerializeField]
    private float effectScale = 1.2f; // 숫자가 바뀔 때 커지는 배율

    [Header("Question")]
    [SerializeField]
    private TMP_Text questionText;

    [Header("Choices (Concept/Idea용)")]
    [SerializeField]
    private Button[] choiceButtons = new Button[4];

    [SerializeField]
    private TMP_Text[] choiceTexts = new TMP_Text[4];

    [Header("Design Mode UI (Panel_Design용)")]
    [SerializeField]
    private Button[] designChoiceButtons = new Button[4];

    [SerializeField]
    private TMP_Text[] designChoiceTexts = new TMP_Text[4];

    [SerializeField]
    private Button designSubmitButton;

    [Header("Input Mode (OCR)")]
    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private Button submitButton;

    [Header("UI References")]
    [SerializeField]
    private ExperienceDifficultyBar difficultyBar;

    [SerializeField]
    private ExperienceResultUI resultUI;

    [SerializeField]
    private ArenaResultUI arenaResultUI;

    [SerializeField]
    private TrainingResultUI trainingResultUI; // [추가] 훈련장용 결과창

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
    private TMP_Text verifiedDigitText;
    private int lastPredictedValue = -1;

    private CommonCategory currentCategory;
    private int correctChoiceIndex = 0;
    private string correctAnswerText = "";
    private int correctCount = 0;
    private int totalQuestionLimit = 30;

    private List<int> designUserSequence = new List<int>();
    private int[] designCorrectSequence;
    private string[] designOriginalTexts;

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
        HookAllButtons();

        // 첫 카운트 UI 업데이트
        UpdateQuestionCounterUI(false);
    }

    public void OnTimeOut() => ProcessAnswer(false);

    private CommonCategory ReadCurrentCategory()
    {
        if (mode == BattleMode.Arena)
        {
            return ArenaSession.CurrentCategory switch
            {
                ArenaCategory.Concept => CommonCategory.Concept,
                ArenaCategory.Calc => CommonCategory.Calc,
                ArenaCategory.Idea => CommonCategory.Idea,
                ArenaCategory.Design => CommonCategory.Design,
                ArenaCategory.Practice => CommonCategory.Practice,
                _ => CommonCategory.Concept,
            };
        }
        else if (mode == BattleMode.Training)
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
        if (panelDesign != null)
            panelDesign.SetActive(currentCategory == CommonCategory.Design);
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
            case CommonCategory.Design:
                SetupDesignQuestion();
                break;
            default:
                SetupConceptQuestion();
                break;
        }
    }

    private void SetupConceptQuestion()
    {
        questionText.text = "개념이해 예시";
        correctChoiceIndex = 0;
    }

    private void SetupIdeaQuestion()
    {
        questionText.text = "발상 예시";
        correctChoiceIndex = 1;
    }

    private void SetupCalcQuestion()
    {
        questionText.text = "연산 예시";
        correctAnswerText = "5";
    }

    private void SetupPracticeQuestion()
    {
        questionText.text = "실전 예시";
        correctAnswerText = "7";
    }

    private void SetupDesignQuestion()
    {
        questionText.text =
            "이차함수 $y = x^2 - 4x + 3$의 꼭짓점 좌표를 구하는 올바른 순서를 설계하시오.";
        designOriginalTexts = new string[] { "식 묶기", "좌표 도출", "상수 조절", "표준형 정리" };
        designCorrectSequence = new int[] { 0, 2, 3, 1 };
        designUserSequence.Clear();
        for (int i = 0; i < designChoiceTexts.Length; i++)
        {
            if (i < designOriginalTexts.Length)
            {
                designChoiceTexts[i].text = designOriginalTexts[i];
                designChoiceButtons[i].gameObject.SetActive(true);
                designChoiceButtons[i].image.color = Color.white;
            }
            else
                designChoiceButtons[i].gameObject.SetActive(false);
        }
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.Hard);
    }

    // --- 카운트 UI 연출 (Coroutine) ---

    private void UpdateQuestionCounterUI(bool playEffect)
    {
        if (questionCounterText == null)
            return;

        int currentNum = ExperienceSession.CurrentQuestionCount + 1;
        questionCounterText.text = $"{currentNum} / {totalQuestionLimit}";

        if (playEffect)
        {
            StopCoroutine("ScaleEffectRoutine");
            StartCoroutine("ScaleEffectRoutine");
        }
    }

    private IEnumerator ScaleEffectRoutine()
    {
        Transform trans = questionCounterText.transform;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * effectScale;

        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            trans.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / 0.1f);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            trans.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / 0.1f);
            yield return null;
        }
        trans.localScale = originalScale;
    }

    // --- 정답 판정 및 종료 처리 ---

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

        if (
            mode == BattleMode.Experience
            && ExperienceSession.CurrentQuestionCount >= totalQuestionLimit
        )
            FinishBattle();
        else if (mode != BattleMode.Experience && ExperienceSession.CurrentLife <= 0)
            FinishBattle();
        else
        {
            UpdateQuestionCounterUI(true); // 다음 문제로 갈 때 연출
            Object.FindFirstObjectByType<ExperienceBattleAppBar>()?.ResetTimer();
            SetupQuestionByCategory();
        }
    }

    public void FinishBattle()
    {
        if (mode == BattleMode.Arena)
        {
            bool isWin = ExperienceSession.CurrentLife > 0;
            int arChange = CalculateArenaRatingChange(isWin);
            if (arenaResultUI != null)
                arenaResultUI.Show(isWin, 1247, 1247 + arChange, arChange, "레전드 브론즈", null);
        }
        else if (mode == BattleMode.Training)
        {
            // 훈련장 결과: 전투력(BP)과 해금 메시지 전달
            int finalBP = ExperienceSession.TotalExpScore;
            string unlocked = "";
            if (finalBP >= 200)
                unlocked = "HARD, VERY HARD";
            else if (finalBP >= 100)
                unlocked = "NORMAL";

            if (trainingResultUI != null)
                trainingResultUI.Show(finalBP, unlocked);
        }
        else
        {
            if (resultUI != null)
                resultUI.Show(
                    correctCount,
                    ExperienceSession.CurrentQuestionCount,
                    ExperienceSession.TotalExpScore
                );
        }
    }

    // --- (이하 기존 공통 로직 유지) ---

    private int CalculateArenaRatingChange(bool isWin)
    {
        int myBP = ArenaSession.GetPlayerBP(ArenaSession.CurrentCategory);
        int opBP = ArenaSession.OpponentRating;
        int diff = Mathf.Abs(myBP - opBP);
        if (isWin)
            return (diff > 100) ? 15 : (diff >= 50 ? 10 : 5);
        else
            return (diff > 100) ? -5 : (diff >= 50 ? -10 : -15);
    }

    private void UpdateLifeUI()
    {
        if (lifeText != null)
            lifeText.text = $"Life: {ExperienceSession.CurrentLife}";
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
                heartIcons[i].sprite =
                    (i < ExperienceSession.CurrentLife) ? activeHeartSprite : inactiveHeartSprite;
        }
    }

    public void OnClickSubmitAnswer()
    {
        if (currentCategory == CommonCategory.Design)
        {
            if (designUserSequence.Count < designCorrectSequence.Length)
                return;
            CheckDesignAnswer();
        }
        else
        {
            if (lastPredictedValue == -1)
                return;
            ProcessAnswer(lastPredictedValue.ToString() == correctAnswerText);
            OnClickClearCanvas();
        }
    }

    private void CheckDesignAnswer()
    {
        bool isCorrect = true;
        for (int i = 0; i < designCorrectSequence.Length; i++)
        {
            if (designUserSequence[i] != designCorrectSequence[i])
            {
                isCorrect = false;
                break;
            }
        }
        ProcessAnswer(isCorrect);
        designUserSequence.Clear();
    }

    private void OnClickChoice(int index)
    {
        if (currentCategory == CommonCategory.Design)
            HandleDesignSelection(index);
        else
            ProcessAnswer(index == correctChoiceIndex);
    }

    private void HandleDesignSelection(int index)
    {
        if (designUserSequence.Contains(index))
            designUserSequence.Remove(index);
        else if (designUserSequence.Count < designCorrectSequence.Length)
            designUserSequence.Add(index);
        UpdateDesignUI();
    }

    private void UpdateDesignUI()
    {
        for (int i = 0; i < designOriginalTexts.Length; i++)
        {
            int pos = designUserSequence.IndexOf(i);
            if (pos != -1)
            {
                designChoiceTexts[i].text =
                    $"<color=#00FF00>{pos + 1}.</color> {designOriginalTexts[i]}";
                designChoiceButtons[i].image.color = Color.cyan;
            }
            else
            {
                designChoiceTexts[i].text = designOriginalTexts[i];
                designChoiceButtons[i].image.color = Color.white;
            }
        }
    }

    public void OnClickVerifyDigit()
    {
        if (drawingCanvas == null || ocrManager == null)
            return;
        lastPredictedValue = ocrManager.PredictDigit(drawingCanvas.GetCapturedTexture());
        if (verifiedDigitText != null)
            verifiedDigitText.text = $"입력된 숫자 : {lastPredictedValue}";
    }

    public void OnClickClearCanvas()
    {
        drawingCanvas.ClearCanvas();
        lastPredictedValue = -1;
        if (verifiedDigitText != null)
            verifiedDigitText.text = "입력된 숫자 : ";
    }

    private void HookAllButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null)
                continue;
            int idx = i;
            choiceButtons[idx].onClick.RemoveAllListeners();
            choiceButtons[idx].onClick.AddListener(() => OnClickChoice(idx));
        }
        for (int i = 0; i < designChoiceButtons.Length; i++)
        {
            if (designChoiceButtons[i] == null)
                continue;
            int idx = i;
            designChoiceButtons[idx].onClick.RemoveAllListeners();
            designChoiceButtons[idx].onClick.AddListener(() => OnClickChoice(idx));
        }
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnClickSubmitAnswer);
        }
        if (designSubmitButton != null)
        {
            designSubmitButton.onClick.RemoveAllListeners();
            designSubmitButton.onClick.AddListener(OnClickSubmitAnswer);
        }
    }
}
