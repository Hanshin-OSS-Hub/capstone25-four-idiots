using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBattleController : MonoBehaviour
{
    public enum BattleMode { Experience, Training, Arena }

    [Header("Mode")]
    [SerializeField] private BattleMode mode = BattleMode.Experience;

    [Header("Panels")]
    [SerializeField] private GameObject panelChoices; 
    [SerializeField] private GameObject panelInput;   

    [Header("Question")]
    [SerializeField] private TMP_Text questionText; 

    [Header("Choices (4지선다)")]
    [SerializeField] private Button[] choiceButtons = new Button[4];
    [SerializeField] private TMP_Text[] choiceTexts = new TMP_Text[4];

    [Header("Input Mode")]
    [SerializeField] private TMP_InputField inputField; 
    [SerializeField] private Button submitButton; 

    [Header("UI References")]
    [SerializeField] private ExperienceDifficultyBar difficultyBar;
    [SerializeField] private ExperienceResultUI resultUI; 

    private CommonCategory currentCategory;
    private int correctChoiceIndex = 0;
    private string correctAnswerText = "";
    private int correctCount = 0; 

    private enum CommonCategory { Concept, Calc, Idea, Design, Practice }

    private void Awake()
    {
        // 세션 초기화
        ExperienceSession.TotalExpScore = 0;
        ExperienceSession.CurrentQuestionCount = 0;

        currentCategory = ReadCurrentCategory();
        SetupModeByCategory();
        SetupQuestionByCategory();
        HookChoiceButtons();
        HookSubmitButton();
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
        if (panelChoices != null) panelChoices.SetActive(currentCategory == CommonCategory.Concept || currentCategory == CommonCategory.Idea);
        if (panelInput != null) panelInput.SetActive(currentCategory == CommonCategory.Calc || currentCategory == CommonCategory.Practice);
    }

    private void SetupQuestionByCategory()
    {
        switch (currentCategory)
        {
            case CommonCategory.Concept: SetupConceptQuestion(); break;
            case CommonCategory.Idea: SetupIdeaQuestion(); break;
            case CommonCategory.Calc: SetupCalcQuestion(); break;
            case CommonCategory.Practice: SetupPracticeQuestion(); break;
            default: SetupDesignQuestion(); break;
        }
    }

    // --- 카테고리별 문제 셋업 (오류 해결용 함수들) ---

    private void SetupConceptQuestion()
    {
        if (questionText != null) questionText.text = "변수의 값에 따라 참이 되기도 하고, 거짓이 되기도 하는 등식";
        SetChoiceText(0, "1. 항등식"); SetChoiceText(1, "2. 부등식"); SetChoiceText(2, "3. 함수"); SetChoiceText(3, "4. 방정식");
        correctChoiceIndex = 3;
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.VeryEasy);
    }

    private void SetupIdeaQuestion()
    {
        if (questionText != null) questionText.text = "다음 두 다항식의 공통인수는?\nab+a, 2ab+2a";
        SetChoiceText(0, "1. 곱셈공식"); SetChoiceText(1, "2. 제곱근의 정의"); SetChoiceText(2, "3. 인수분해"); SetChoiceText(3, "4. 나머지정리");
        correctChoiceIndex = 2;
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.Hard);
    }

    private void SetupCalcQuestion()
    {
        if (questionText != null) questionText.text = "다음 이차방정식의 두 근의 합은?\n2x^2 + 6x + 3 = 0";
        correctAnswerText = "-3";
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.VeryEasy);
    }

    private void SetupPracticeQuestion()
    {
        if (questionText != null) questionText.text = "12x^2 - ax - 12가 4x+3을 인수로 가질 때, 상수 a의 값은?";
        correctAnswerText = "7";
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.VeryEasy);
    }

    private void SetupDesignQuestion()
    {
        if (questionText != null) questionText.text = "설계 종목 예시 문제입니다.";
        correctAnswerText = "0";
        difficultyBar?.ApplyDifficulty(ExperienceDifficulty.Easy);
    }

    // --- 정답 체크 및 종료 로직 ---

    private void OnClickChoice(int index)
    {
        bool isCorrect = (index == correctChoiceIndex);
        ProcessAnswer(isCorrect);
    }

    private void OnClickSubmitAnswer()
    {
        string userAnswer = inputField != null ? inputField.text.Trim() : "";
        bool isCorrect = (userAnswer == correctAnswerText);
        ProcessAnswer(isCorrect);
    }

    private void ProcessAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            correctCount++;
            ExperienceSession.TotalExpScore += (int)ExperienceSession.CurrentDifficulty * 100;
        }

        ExperienceSession.CurrentQuestionCount++;
        FinishExperience();
    }

    public void FinishExperience()
    {
        if (resultUI != null)
        {
            resultUI.Show(correctCount, ExperienceSession.CurrentQuestionCount, ExperienceSession.TotalExpScore);
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
            choiceButtons[i]?.onClick.RemoveAllListeners(); // 중복 리스너 방지
            choiceButtons[i]?.onClick.AddListener(() => OnClickChoice(idx));
        }
    }

    private void HookSubmitButton()
    {
        if (submitButton == null) return;
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(OnClickSubmitAnswer);
    }
}