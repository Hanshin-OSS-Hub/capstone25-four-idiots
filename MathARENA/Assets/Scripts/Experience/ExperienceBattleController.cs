using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBattleController : MonoBehaviour
{
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

    private ExperienceCategory currentCategory;

    private int correctChoiceIndex = 0; // 객관식 정답 인덱스
    private string correctAnswerText = ""; // 주관식/OCR 정답 텍스트

    private void Awake()
    {
        // 1) 어떤 종목(개념이해/연산/…)으로 들어왔는지 읽기
        currentCategory = ExperienceSession.CurrentCategory;

        // 2) 카테고리에 따라 어떤 패널을 켤지, 어떤 문제를 보여줄지 결정
        SetupModeByCategory();
        SetupQuestionByCategory();

        // 3) 버튼 이벤트 바인딩
        HookChoiceButtons();
        HookSubmitButton();
    }

    // --------- 카테고리별 모드(객관식/인풋) 토글 ---------

    private bool IsMultipleChoiceCategory =>
        currentCategory == ExperienceCategory.Concept || currentCategory == ExperienceCategory.Idea; // 발상도 객관식

    private bool IsInputCategory =>
        currentCategory == ExperienceCategory.Calc
        || currentCategory == ExperienceCategory.Practice; // 연산/실전

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
        if (currentCategory == ExperienceCategory.Concept)
        {
            SetupConceptQuestion();
        }
        else if (currentCategory == ExperienceCategory.Idea)
        {
            SetupIdeaQuestion();
        }
        else if (currentCategory == ExperienceCategory.Calc)
        {
            SetupCalcQuestion();
        }
        else if (currentCategory == ExperienceCategory.Practice)
        {
            SetupPracticeQuestion();
        }
        else // Design 등
        {
            SetupDesignQuestion();
        }
    }

    // 개념이해: 4지선다
    private void SetupConceptQuestion()
    {
        if (questionText != null)
            questionText.text = "변수의 값에 따라 참이 되기도 하고, 거짓이 되기도 하는 등식";

        SetChoiceText(0, "1. 항등식");
        SetChoiceText(1, "2. 부등식");
        SetChoiceText(2, "3. 함수");
        SetChoiceText(3, "4. 방정식");

        correctChoiceIndex = 3; // 4번 방정식

        var diff = ExperienceDifficulty.VeryEasy;
        ExperienceSession.CurrentDifficulty = diff;
        difficultyBar?.ApplyDifficulty(diff);
    }

    // 발상: 4지선다 (예시)
    private void SetupIdeaQuestion()
    {
        if (questionText != null)
            questionText.text = "다음 두 다항식의 공통인수는?\nab+a, 2ab+2a";

        SetChoiceText(0, "1. 곱셈공식");
        SetChoiceText(1, "2. 제곱근의 정의");
        SetChoiceText(2, "3. 인수분해");
        SetChoiceText(3, "4. 나머지정리");

        correctChoiceIndex = 2; // 3. 인수분해

        var diff = ExperienceDifficulty.Hard;
        ExperienceSession.CurrentDifficulty = diff;
        difficultyBar?.ApplyDifficulty(diff);
    }

    // 연산: 인풋 패널 + 정답 텍스트
    private void SetupCalcQuestion()
    {
        if (questionText != null)
            questionText.text = "다음 이차방정식의 두 근의 합은?\n2x^2 + 6x + 3 = 0";

        correctAnswerText = "-3";

        var diff = ExperienceDifficulty.VeryEasy;
        ExperienceSession.CurrentDifficulty = diff;
        difficultyBar?.ApplyDifficulty(diff);
    }

    // 실전: 인풋 패널 + 정답 텍스트 (예시)
    private void SetupPracticeQuestion()
    {
        if (questionText != null)
            questionText.text = "12x^2 - ax - 12가 4x+3을 인수로 가질 때, 상수 a의 값은?";

        correctAnswerText = "7";

        var diff = ExperienceDifficulty.VeryEasy;
        ExperienceSession.CurrentDifficulty = diff;
        difficultyBar?.ApplyDifficulty(diff);
    }

    // 설계: 임시 더미
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

    // 객관식용
    private void OnClickChoice(int index)
    {
        if (!IsMultipleChoiceCategory)
            return; // 안전장치

        bool isCorrect = (index == correctChoiceIndex);
        Debug.Log($"[체험장-{currentCategory}] 선택지 {index + 1}번, 정답 여부: {isCorrect}");
    }

    // 인풋/OCR용 (지금은 TMP_InputField 기반, 나중에 OCR 결과로 교체)
    private void OnClickSubmitAnswer()
    {
        if (!IsInputCategory)
            return; // 객관식 카테고리에서는 무시

        string userAnswer = GetUserAnswerText();
        bool isCorrect = (userAnswer == correctAnswerText);

        Debug.Log(
            $"[체험장-{currentCategory}] 입력답안={userAnswer}, 정답={correctAnswerText}, 정답 여부: {isCorrect}"
        );
    }

    // 나중에 OCR 붙일 때 여기만 수정하면 됨
    private string GetUserAnswerText()
    {
        if (inputField != null)
            return inputField.text.Trim();

        return string.Empty;
    }
}
