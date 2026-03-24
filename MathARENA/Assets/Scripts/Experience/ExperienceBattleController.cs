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
        HookChoiceButtons(); // 버튼 연결 함수
        HookSubmitButton();
    }

    public void OnTimeOut()
    {
        ProcessAnswer(false);
    }

    private CommonCategory ReadCurrentCategory()
    {
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

    private void SetupConceptQuestion()
    {
        if (questionText != null)
            questionText.text = "변수의 값에 따라 참이 되기도 하고, 거짓이 되기도 하는 등식";
        SetChoiceText(0, "1. 항등식");
        SetChoiceText(1, "2. 부등식");
        SetChoiceText(2, "3. 함수");
        SetChoiceText(3, "4. 방정식");
        correctChoiceIndex = 3;
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.VeryEasy);
    }

    private void SetupIdeaQuestion()
    {
        if (questionText != null)
            questionText.text = "다음 두 다항식의 공통인수는?\nab+a, 2ab+2a";
        SetChoiceText(0, "1. 곱셈공식");
        SetChoiceText(1, "2. 제곱근의 정의");
        SetChoiceText(2, "3. 인수분해");
        SetChoiceText(3, "4. 나머지정리");
        correctChoiceIndex = 2;
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.Hard);
    }

    private void SetupCalcQuestion()
    {
        correctAnswerText = "-3";
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.VeryEasy);
    }

    private void SetupPracticeQuestion()
    {
        correctAnswerText = "7";
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.VeryEasy);
    }

    private void SetupDesignQuestion()
    {
        correctAnswerText = "0";
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.Easy);
    }

    private void OnClickChoice(int index)
    {
        ProcessAnswer(index == correctChoiceIndex);
    }

    private void OnClickSubmitAnswer()
    {
        string userAnswer = inputField != null ? inputField.text.Trim() : "";
        ProcessAnswer(userAnswer == correctAnswerText);
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
        {
            FinishExperience();
        }
        else if (mode == BattleMode.Experience)
        {
            FinishExperience();
        }
        else
        {
            Object.FindFirstObjectByType<ExperienceBattleAppBar>().ResetTimer();
            SetupQuestionByCategory();
        }
    }

    public void FinishExperience()
    {
        if (resultUI != null)
        {
            resultUI.Show(
                correctCount,
                ExperienceSession.CurrentQuestionCount,
                ExperienceSession.TotalExpScore
            );
        }
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
                {
                    heartIcons[i].sprite =
                        (i < ExperienceSession.CurrentLife)
                            ? activeHeartSprite
                            : inactiveHeartSprite;
                }
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
