using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBattleController : MonoBehaviour
{
    public enum BattleMode
    {
        Experience,
        Training,
        Arena, // 추후 확장
    }

    [Header("Mode")]
    [SerializeField]
    private BattleMode mode = BattleMode.Experience;

    [Header("Panels")]
    [SerializeField]
    private GameObject panelChoices; // Panel_Choices

    [SerializeField]
    private GameObject panelInput; // Panel_Input (연산/실전)

    [Header("Question")]
    [SerializeField]
    private TMP_Text questionText; // Text_Question

    [Header("Choices (4지선다)")]
    [SerializeField]
    private Button[] choiceButtons = new Button[4];

    [SerializeField]
    private TMP_Text[] choiceTexts = new TMP_Text[4];

    [Header("Input Mode")]
    [SerializeField]
    private TMP_InputField inputField; // 지금은 텍스트 입력용(나중에 OCR로 교체)

    [SerializeField]
    private Button submitButton; // Button_Submit

    [Header("Difficulty Bar")]
    [SerializeField]
    private ExperienceDifficultyBar difficultyBar;

    // 공용 카테고리로 한 번 통일해서 처리
    private CommonCategory currentCategory;

    private int correctChoiceIndex = 0;
    private string correctAnswerText = "";

    // 공용 카테고리
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
        // 1) 모드별로 현재 카테고리 읽기
        currentCategory = ReadCurrentCategory();

        // 2) 카테고리에 따라 패널/문제 세팅
        SetupModeByCategory();
        SetupQuestionByCategory();

        // 3) 버튼 이벤트 바인딩 (중복 리스너 방지)
        HookChoiceButtons();
        HookSubmitButton();
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
                return ArenaSession.CurrentCategory switch
                {
                    ArenaCategory.Concept => CommonCategory.Concept,
                    ArenaCategory.Calc => CommonCategory.Calc,
                    ArenaCategory.Idea => CommonCategory.Idea,
                    ArenaCategory.Design => CommonCategory.Design,
                    ArenaCategory.Practice => CommonCategory.Practice,
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

    // --------- 카테고리별 모드(객관식/인풋) 토글 ---------

    private bool IsMultipleChoiceCategory =>
        currentCategory == CommonCategory.Concept || currentCategory == CommonCategory.Idea;

    private bool IsInputCategory =>
        currentCategory == CommonCategory.Calc || currentCategory == CommonCategory.Practice;

    private void SetupModeByCategory()
    {
        if (panelChoices != null)
            panelChoices.SetActive(IsMultipleChoiceCategory);

        if (panelInput != null)
            panelInput.SetActive(IsInputCategory);
    }

    // --------- 카테고리별 문제 세팅 ---------

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
            default:
                SetupDesignQuestion();
                break;
        }
    }

    private string ModePrefix =>
        mode switch
        {
            BattleMode.Training => "훈련장",
            BattleMode.Arena => "아레나",
            _ => "체험장",
        };

    // 개념이해: 4지선다
    private void SetupConceptQuestion()
    {
        if (questionText != null)
            questionText.text = "변수의 값에 따라 참이 되기도 하고, 거짓이 되기도 하는 등식";

        SetChoiceText(0, "1. 항등식");
        SetChoiceText(1, "2. 부등식");
        SetChoiceText(2, "3. 함수");
        SetChoiceText(3, "4. 방정식");

        correctChoiceIndex = 3;

        var diff = ExperienceDifficulty.VeryEasy;
        ExperienceSession.CurrentDifficulty = diff; // 지금은 공용 난이도 바를 그대로 쓰는 전제
        difficultyBar?.ApplyDifficulty(diff);
    }

    // 발상: 4지선다
    private void SetupIdeaQuestion()
    {
        if (questionText != null)
            questionText.text = "다음 두 다항식의 공통인수는?\nab+a, 2ab+2a";

        SetChoiceText(0, "1. 곱셈공식");
        SetChoiceText(1, "2. 제곱근의 정의");
        SetChoiceText(2, "3. 인수분해");
        SetChoiceText(3, "4. 나머지정리");

        correctChoiceIndex = 2;

        var diff = ExperienceDifficulty.Hard;
        ExperienceSession.CurrentDifficulty = diff;
        difficultyBar?.ApplyDifficulty(diff);
    }

    // 연산: 인풋
    private void SetupCalcQuestion()
    {
        if (questionText != null)
            questionText.text = "다음 이차방정식의 두 근의 합은?\n2x^2 + 6x + 3 = 0";

        correctAnswerText = "-3";

        var diff = ExperienceDifficulty.VeryEasy;
        ExperienceSession.CurrentDifficulty = diff;
        difficultyBar?.ApplyDifficulty(diff);
    }

    // 실전: 인풋
    private void SetupPracticeQuestion()
    {
        if (questionText != null)
            questionText.text = "12x^2 - ax - 12가 4x+3을 인수로 가질 때, 상수 a의 값은?";

        correctAnswerText = "7";

        var diff = ExperienceDifficulty.VeryEasy;
        ExperienceSession.CurrentDifficulty = diff;
        difficultyBar?.ApplyDifficulty(diff);
    }

    // 설계: 임시
    private void SetupDesignQuestion()
    {
        if (questionText != null)
            questionText.text = "설계 종목 예시 문제입니다.";

        correctAnswerText = "0";

        var diff = ExperienceDifficulty.Easy;
        ExperienceSession.CurrentDifficulty = diff;
        difficultyBar?.ApplyDifficulty(diff);
    }

    // --------- 공통 유틸 ---------

    private void SetChoiceText(int index, string text)
    {
        if (index < 0 || index >= choiceTexts.Length)
            return;
        if (choiceTexts[index] != null)
            choiceTexts[index].text = text;
    }

    private void HookChoiceButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null)
                continue;
            int idx = i;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnClickChoice(idx));
        }
    }

    private void HookSubmitButton()
    {
        if (submitButton == null)
            return;
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(OnClickSubmitAnswer);
    }

    private void OnClickChoice(int index)
    {
        if (!IsMultipleChoiceCategory)
            return;

        bool isCorrect = (index == correctChoiceIndex);
        Debug.Log($"[{ModePrefix}-{currentCategory}] 선택지 {index + 1}번, 정답 여부: {isCorrect}");
    }

    private void OnClickSubmitAnswer()
    {
        if (!IsInputCategory)
            return;

        string userAnswer = GetUserAnswerText();
        bool isCorrect = (userAnswer == correctAnswerText);

        Debug.Log(
            $"[{ModePrefix}-{currentCategory}] 입력답안={userAnswer}, 정답={correctAnswerText}, 정답 여부: {isCorrect}"
        );
    }

    private string GetUserAnswerText()
    {
        if (inputField != null)
            return inputField.text.Trim();
        return string.Empty;
    }
}
