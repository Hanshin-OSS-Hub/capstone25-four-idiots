using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathArena.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBattleController : MonoBehaviour
{
    private class ChoiceData
    {
        public string text;
        public string originalIndex;
    }

    public enum BattleMode
    {
        Experience,
        Training,
        Arena,
    }

    [Header("Mode Settings")]
    [SerializeField]
    private BattleMode mode = BattleMode.Experience;

    [Header("UI Panels")]
    [SerializeField]
    private GameObject panelChoices;

    [SerializeField]
    private GameObject panelDesign;

    [SerializeField]
    private GameObject panelInput;

    [Header("External Managers")]
    [SerializeField]
    private BattleSequenceManager sequenceManager;

    [SerializeField]
    private ExperienceDifficultyBar difficultyBar;

    [Header("Question UI")]
    [SerializeField]
    private TMP_Text questionCounterText;

    [SerializeField]
    private TMP_Text questionText;

    [Header("Choices (Concept/Idea)")]
    [SerializeField]
    private Button[] choiceButtons = new Button[4];

    [SerializeField]
    private TMP_Text[] choiceTexts = new TMP_Text[4];

    [Header("Design Mode UI")]
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

    [Header("Result UIs")]
    [SerializeField]
    private ExperienceResultUI resultUI;

    [SerializeField]
    private ArenaResultUI arenaResultUI;

    [SerializeField]
    private TrainingResultUI trainingResultUI;

    [Header("Life UI")]
    [SerializeField]
    private TMP_Text lifeText;

    [SerializeField]
    private Image[] heartIcons = new Image[4];

    [SerializeField]
    private Sprite activeHeartSprite;

    [SerializeField]
    private Sprite inactiveHeartSprite;

    [Header("OCR Systems")]
    [SerializeField]
    private SentisOCRManager ocrManager;

    [SerializeField]
    private DrawingCanvas drawingCanvas;

    private List<QuestionResultData> battleRecords = new List<QuestionResultData>();
    private List<string> solvedQuestionIds = new List<string>();
    private float questionStartTime;
    private string currentQuestionId;

    // 에러 해결: 클래스 멤버로 선언
    private string currentDiffName = "VERY EASY";

    private CommonCategory currentCategory;
    private string correctAnswerText = "";
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
        InitializeBattle();
        HookAllButtons();
        UpdateQuestionCounterUI();
    }

    private void InitializeBattle()
    {
        currentCategory = ReadCurrentCategory();
        SetTotalQuestionLimit();

        ExperienceSession.CurrentLife = ExperienceSession.MaxLife;
        UpdateLifeUI();

        ExperienceSession.TotalExpScore = 0;
        ExperienceSession.CurrentQuestionCount = 0;

        battleRecords.Clear();
        solvedQuestionIds.Clear();

        SetupModeByCategory();
        SetupQuestionByCategory();
    }

    private void SetTotalQuestionLimit()
    {
        int cp = GetCurrentCategoryCP();
        if (cp >= 500)
            totalQuestionLimit = 50;
        else if (cp >= 200)
            totalQuestionLimit = 40;
        else
            totalQuestionLimit = 30;
    }

    private int GetCurrentCategoryCP()
    {
        // [수정] Profile -> UserProfile (에러 CS0117 해결)
        var p = ExperienceSession.UserProfile;
        if (p == null)
            return 0;

        switch (currentCategory)
        {
            case CommonCategory.Concept:
                return p.cp_concept;
            case CommonCategory.Calc:
                return p.cp_calc;
            case CommonCategory.Idea:
                return p.cp_idea;
            case CommonCategory.Design:
                return p.cp_design;
            default:
                return p.cp_practical;
        }
    }

    private CommonCategory ReadCurrentCategory()
    {
        if (mode == BattleMode.Arena)
        {
            switch (ArenaSession.CurrentCategory)
            {
                case ArenaCategory.Concept:
                    return CommonCategory.Concept;
                case ArenaCategory.Calc:
                    return CommonCategory.Calc;
                case ArenaCategory.Idea:
                    return CommonCategory.Idea;
                case ArenaCategory.Design:
                    return CommonCategory.Design;
                case ArenaCategory.Practice:
                    return CommonCategory.Practice;
                default:
                    return CommonCategory.Concept;
            }
        }
        return CommonCategory.Concept;
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
        string difficulty = DetermineDifficulty();
        string category = currentCategory.ToString();
        string excludeList = string.Join(",", solvedQuestionIds.ToArray());

        // [수정] 인자 5개를 전달하도록 변경 (에러 CS1501 해결)
        NetworkManager.Instance.GetQuestion(
            category,
            difficulty,
            excludeList,
            (response) =>
            {
                if (response.success && response.data != null)
                {
                    solvedQuestionIds.Add(response.data.q_id);
                    ApplyServerDataToUI(response.data);
                }
            },
            (error) => Debug.LogError(error)
        );
    }

    private void OnQuestionLoaded(AuthResponse<ServerQuestionData> response)
    {
        if (response.success && response.data != null)
        {
            solvedQuestionIds.Add(response.data.q_id);
            ApplyServerDataToUI(response.data);
        }
    }

    private void OnQuestionLoadFailed(string error)
    {
        Debug.LogError("네트워크 통신 실패: " + error);
    }

    private string DetermineDifficulty()
    {
        int qIdx = ExperienceSession.CurrentQuestionCount + 1;
        int cp = GetCurrentCategoryCP();

        if (cp >= 500)
        {
            if (qIdx <= 5)
                return "VERY EASY";
            if (qIdx <= 10)
                return "EASY";
            if (qIdx <= 15)
                return "HARD";
            if (qIdx <= 20)
                return "VERY HARD";
            if (qIdx <= 30)
                return "TOUGH";
            return "VERY TOUGH";
        }
        else if (cp >= 200)
        {
            if (qIdx <= 10)
                return "VERY EASY";
            if (qIdx <= 20)
                return "EASY";
            if (qIdx <= 30)
                return "HARD";
            return "VERY HARD";
        }
        return qIdx <= 20 ? "VERY EASY" : "EASY";
    }

    private void ApplyServerDataToUI(ServerQuestionData data)
    {
        questionText.text = data.content;
        currentQuestionId = data.q_id;
        currentDiffName = data.diff_name;
        questionStartTime = Time.time;

        if (difficultyBar != null)
            difficultyBar.ApplyDifficulty(ParseDifficulty(data.diff_name));

        if (currentCategory == CommonCategory.Concept || currentCategory == CommonCategory.Idea)
        {
            List<ChoiceData> choices = new List<ChoiceData>();
            choices.Add(new ChoiceData { text = data.opt1, originalIndex = "0" });
            choices.Add(new ChoiceData { text = data.opt2, originalIndex = "1" });
            choices.Add(new ChoiceData { text = data.opt3, originalIndex = "2" });
            choices.Add(new ChoiceData { text = data.opt4, originalIndex = "3" });

            Shuffle(choices);

            for (int i = 0; i < 4; i++)
            {
                choiceTexts[i].text = choices[i].text;
                if (choices[i].originalIndex == data.answer)
                    correctAnswerText = i.ToString();
            }
        }
        else if (
            currentCategory == CommonCategory.Calc
            || currentCategory == CommonCategory.Practice
        )
        {
            correctAnswerText = data.ocr_answer;
        }
        else if (currentCategory == CommonCategory.Design)
        {
            List<string> designChoices = new List<string>
            {
                data.opt1,
                data.opt2,
                data.opt3,
                data.opt4,
            };
            Shuffle(designChoices);
            designOriginalTexts = designChoices.ToArray();
            designCorrectSequence = data.order_answer.Split(',').Select(int.Parse).ToArray();
            designUserSequence.Clear();
            UpdateDesignUI();
        }
    }

    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void OnTimeOut()
    {
        ProcessAnswer(false);
    }

    private void ProcessAnswer(bool isCorrect)
    {
        float solveTime = Time.time - questionStartTime;
        if (solveTime > 60f)
            isCorrect = false;

        QuestionResultData record = new QuestionResultData
        {
            q_id = currentQuestionId,
            is_correct = isCorrect,
            // [수정] Time.Min -> Mathf.Min (에러 CS0117 해결)
            solve_time_sec = Mathf.RoundToInt(Mathf.Min(solveTime, 60f)),
        };
        battleRecords.Add(record);

        if (isCorrect)
            ExperienceSession.TotalExpScore += GetDifficultyScore(ParseDifficulty(currentDiffName));
        else
        {
            ExperienceSession.CurrentLife--;
            UpdateLifeUI();
        }

        if (sequenceManager != null)
        {
            sequenceManager.OnSequenceComplete = ContinueBattleProcess;
            sequenceManager.PlaySequence(isCorrect);
        }
        else
            ContinueBattleProcess();
    }

    private void ContinueBattleProcess()
    {
        ExperienceSession.CurrentQuestionCount++;
        if (
            ExperienceSession.CurrentLife <= 0
            || ExperienceSession.CurrentQuestionCount >= totalQuestionLimit
        )
            FinishBattle();
        else
        {
            UpdateQuestionCounterUI();
            SetupQuestionByCategory();
        }
    }

    private void FinishBattle()
    {
        BattleResultRequest request = new BattleResultRequest
        {
            category_name = currentCategory.ToString(),
            total_score = ExperienceSession.TotalExpScore,
            results = battleRecords,
        };

        NetworkManager.Instance.SaveBattleResult(request, OnSaveSuccess, OnSaveFail);
    }

    private void OnSaveSuccess(AuthResponse<string> response)
    {
        if (response.success)
            ShowResultUI();
    }

    private void OnSaveFail(string error)
    {
        Debug.LogError("결과 저장 실패: " + error);
        ShowResultUI();
    }

    private void ShowResultUI()
    {
        if (mode == BattleMode.Experience)
            resultUI.gameObject.SetActive(true);
        else if (mode == BattleMode.Arena)
            arenaResultUI.gameObject.SetActive(true);
        else
            trainingResultUI.gameObject.SetActive(true);
    }

    private void UpdateLifeUI()
    {
        if (lifeText != null)
            lifeText.text = ExperienceSession.CurrentLife + " / " + ExperienceSession.MaxLife;
        for (int i = 0; i < heartIcons.Length; i++)
            heartIcons[i].sprite =
                (i < ExperienceSession.CurrentLife) ? activeHeartSprite : inactiveHeartSprite;
    }

    private void UpdateQuestionCounterUI()
    {
        if (questionCounterText != null)
            questionCounterText.text =
                (ExperienceSession.CurrentQuestionCount + 1) + " / " + totalQuestionLimit;
    }

    private void UpdateDesignUI()
    {
        for (int i = 0; i < designChoiceButtons.Length; i++)
        {
            designChoiceTexts[i].text = designOriginalTexts[i];
            designChoiceButtons[i].interactable = !designUserSequence.Contains(i);
        }
    }

    private void HookAllButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            choiceButtons[index].onClick.RemoveAllListeners();
            choiceButtons[index]
                .onClick.AddListener(
                    delegate
                    {
                        ProcessAnswer(index.ToString() == correctAnswerText);
                    }
                );
        }

        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(
                delegate
                {
                    ProcessAnswer(inputField.text == correctAnswerText);
                }
            );
        }

        if (designSubmitButton != null)
        {
            designSubmitButton.onClick.RemoveAllListeners();
            designSubmitButton.onClick.AddListener(
                delegate
                {
                    ProcessAnswer(designUserSequence.SequenceEqual(designCorrectSequence));
                }
            );
        }

        for (int i = 0; i < designChoiceButtons.Length; i++)
        {
            int index = i;
            designChoiceButtons[index].onClick.RemoveAllListeners();
            designChoiceButtons[index]
                .onClick.AddListener(
                    delegate
                    {
                        designUserSequence.Add(index);
                        UpdateDesignUI();
                    }
                );
        }
    }

    private ExperienceDifficulty ParseDifficulty(string diffName)
    {
        if (string.IsNullOrEmpty(diffName))
            return ExperienceDifficulty.VeryEasy;
        string normalized = diffName.Replace(" ", "").ToUpper();
        if (normalized == "VERYEASY")
            return ExperienceDifficulty.VeryEasy;
        if (normalized == "EASY")
            return ExperienceDifficulty.Easy;
        if (normalized == "HARD")
            return ExperienceDifficulty.Hard;
        if (normalized == "VERYHARD")
            return ExperienceDifficulty.VeryHard;
        if (normalized == "TOUGH")
            return ExperienceDifficulty.Tough;
        if (normalized == "VERYTOUGH")
            return ExperienceDifficulty.VeryTough;
        return ExperienceDifficulty.VeryEasy;
    }

    private int GetDifficultyScore(ExperienceDifficulty diff)
    {
        if (diff == ExperienceDifficulty.VeryEasy)
            return 5;
        if (diff == ExperienceDifficulty.Easy)
            return 10;
        if (diff == ExperienceDifficulty.Hard)
            return 15;
        if (diff == ExperienceDifficulty.VeryHard)
            return 20;
        if (diff == ExperienceDifficulty.Tough)
            return 25;
        if (diff == ExperienceDifficulty.VeryTough)
            return 30;
        return 0;
    }
}
