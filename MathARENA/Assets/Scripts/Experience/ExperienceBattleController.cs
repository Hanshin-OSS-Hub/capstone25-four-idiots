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

    [Header("Life UI (User)")]
    [SerializeField]
    private GameObject userLifePanel;

    [SerializeField]
    private TMP_Text lifeText;

    [SerializeField]
    private Image[] heartIcons = new Image[4];

    [SerializeField]
    private Sprite activeHeartSprite;

    [SerializeField]
    private Sprite inactiveHeartSprite;

    [Header("Life UI (Opponent - Arena Only)")]
    [SerializeField]
    private GameObject opponentLifePanel;

    [SerializeField]
    private Image[] opponentHeartIcons = new Image[4];

    [SerializeField]
    private Sprite opponentActiveHeartSprite;

    [SerializeField]
    private Sprite opponentInactiveHeartSprite;

    [Header("OCR Systems")]
    [SerializeField]
    private SentisOCRManager ocrManager;

    [SerializeField]
    private DrawingCanvas drawingCanvas;

    // --- 내부 상태 변수 ---
    private List<QuestionResultData> battleRecords = new List<QuestionResultData>();
    private List<QuestionResultData> opponentRecords = new List<QuestionResultData>();
    private List<string> solvedQuestionIds = new List<string>();

    private float questionStartTime;
    private string currentQuestionId;
    private string currentDiffName = "VERY EASY";
    private int opponentLife = 4;
    private int currentOpponentRecordIndex = 0;

    private ExperienceCategory currentCategory;
    private string correctAnswerText = "";
    private int totalQuestionLimit = 30;

    // --- [중요] 연타 방지를 위한 플래그 ---
    private bool isProcessingAnswer = false;

    private List<int> designUserSequence = new List<int>();
    private int[] designCorrectSequence;
    private string[] designOriginalTexts;

    private void Awake()
    {
        InitializeBattle();
        HookAllButtons();
        UpdateQuestionCounterUI();
    }

    private void InitializeBattle()
    {
        currentCategory = ExperienceSession.CurrentCategory;
        SetTotalQuestionLimit();

        ExperienceSession.CurrentLife = ExperienceSession.MaxLife;
        opponentLife = 4;
        isProcessingAnswer = false; // 초기화

        if (userLifePanel != null)
            userLifePanel.SetActive(mode != BattleMode.Experience);
        if (opponentLifePanel != null)
            opponentLifePanel.SetActive(mode == BattleMode.Arena);

        UpdateLifeUI();

        ExperienceSession.TotalExpScore = 0;
        ExperienceSession.CurrentQuestionCount = 0;
        currentOpponentRecordIndex = 0;

        battleRecords.Clear();
        solvedQuestionIds.Clear();

        SetupModeByCategory();

        if (mode == BattleMode.Arena)
            LoadArenaOpponentData();
        else
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
        var p = ExperienceSession.UserProfile;
        if (p == null)
            return 0;
        return currentCategory switch
        {
            ExperienceCategory.Concept => p.cp_concept,
            ExperienceCategory.Calc => p.cp_calc,
            ExperienceCategory.Idea => p.cp_idea,
            ExperienceCategory.Design => p.cp_design,
            ExperienceCategory.Practice => p.cp_practical,
            _ => 0,
        };
    }

    private void SetupModeByCategory()
    {
        panelChoices?.SetActive(
            currentCategory == ExperienceCategory.Concept
                || currentCategory == ExperienceCategory.Idea
        );
        panelDesign?.SetActive(currentCategory == ExperienceCategory.Design);
        panelInput?.SetActive(
            currentCategory == ExperienceCategory.Calc
                || currentCategory == ExperienceCategory.Practice
        );
    }

    private void LoadArenaOpponentData()
    {
        if (NetworkManager.Instance == null)
        {
            SetupQuestionByCategory();
            return;
        }
        NetworkManager.Instance.GetRankingList(
            (res) =>
            {
                SetupQuestionByCategory();
            },
            (err) => Debug.LogError(err)
        );
    }

    private void SetupQuestionByCategory()
    {
        string difficulty = DetermineDifficulty();
        string category = currentCategory.ToString();
        string excludeList = string.Join(",", solvedQuestionIds.ToArray());

        if (NetworkManager.Instance == null)
            return;
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
            (error) =>
            {
                Debug.LogError(error);
                isProcessingAnswer = false; // 에러 시 다시 입력 가능하게 해제
            }
        );
    }

    private string DetermineDifficulty()
    {
        if (mode == BattleMode.Experience)
            return (ExperienceSession.CurrentQuestionCount + 1 <= 20) ? "VERY EASY" : "EASY";

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

        difficultyBar?.ApplyDifficulty(ParseDifficulty(data.diff_name));

        if (
            currentCategory == ExperienceCategory.Concept
            || currentCategory == ExperienceCategory.Idea
        )
        {
            List<ChoiceData> choices = new List<ChoiceData>
            {
                new ChoiceData { text = data.opt1, originalIndex = "0" },
                new ChoiceData { text = data.opt2, originalIndex = "1" },
                new ChoiceData { text = data.opt3, originalIndex = "2" },
                new ChoiceData { text = data.opt4, originalIndex = "3" },
            };
            Shuffle(choices);
            for (int i = 0; i < 4; i++)
            {
                choiceTexts[i].text = choices[i].text;
                if (choices[i].originalIndex == data.answer)
                    correctAnswerText = i.ToString();
            }
        }
        else if (
            currentCategory == ExperienceCategory.Calc
            || currentCategory == ExperienceCategory.Practice
        )
        {
            correctAnswerText = data.ocr_answer;
            drawingCanvas?.ClearCanvas();
            if (inputField != null)
                inputField.text = "";
        }
        else if (currentCategory == ExperienceCategory.Design)
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
            designCorrectSequence = data.order_answer.Split('-').Select(int.Parse).ToArray();
            designUserSequence.Clear();
            UpdateDesignUI();
        }

        // --- [중요] 새로운 문제가 완전히 준비되었을 때만 입력을 다시 허용 ---
        isProcessingAnswer = false;
    }

    public void OnTimeOut() => ProcessAnswer(false);

    private void ProcessAnswer(bool isCorrect)
    {
        // --- [중요] 이미 정답 처리 중이라면 입력을 무시함 ---
        if (isProcessingAnswer)
            return;
        isProcessingAnswer = true;

        float solveTime = Time.time - questionStartTime;
        QuestionResultData userRecord = new QuestionResultData
        {
            q_id = currentQuestionId,
            is_correct = isCorrect,
            solve_time_sec = Mathf.RoundToInt(Mathf.Min(solveTime, 60f)),
        };
        battleRecords.Add(userRecord);

        bool userAttacks = isCorrect;
        bool opponentAttacks = !isCorrect;

        if (mode == BattleMode.Arena && opponentRecords.Count > 0)
        {
            var opRec = opponentRecords[currentOpponentRecordIndex];
            userAttacks =
                isCorrect
                && (!opRec.is_correct || userRecord.solve_time_sec < opRec.solve_time_sec);
            opponentAttacks =
                opRec.is_correct
                && (!isCorrect || opRec.solve_time_sec < userRecord.solve_time_sec);
            currentOpponentRecordIndex = (currentOpponentRecordIndex + 1) % opponentRecords.Count;
        }

        if (userAttacks)
        {
            if (mode == BattleMode.Arena)
                opponentLife--;
            ExperienceSession.TotalExpScore += GetDifficultyScore(ParseDifficulty(currentDiffName));
        }

        if (opponentAttacks && mode != BattleMode.Experience)
            ExperienceSession.CurrentLife--;

        UpdateLifeUI();

        if (sequenceManager != null)
        {
            sequenceManager.OnSequenceComplete = ContinueBattleProcess;
            sequenceManager.PlaySequence(userAttacks);
        }
        else
            ContinueBattleProcess();
    }

    private void ContinueBattleProcess()
    {
        ExperienceSession.CurrentQuestionCount++;
        bool isGameOver;

        if (mode == BattleMode.Experience)
            isGameOver = ExperienceSession.CurrentQuestionCount >= totalQuestionLimit;
        else
            isGameOver =
                ExperienceSession.CurrentLife <= 0
                || (mode == BattleMode.Arena && opponentLife <= 0)
                || ExperienceSession.CurrentQuestionCount >= totalQuestionLimit;

        if (isGameOver)
            FinishBattle();
        else
        {
            UpdateQuestionCounterUI();
            SetupQuestionByCategory();
        }
    }

    private void FinishBattle()
    {
        int finalScore = ExperienceSession.TotalExpScore;
        int previousBP = GetCurrentCategoryCP();

        if (mode == BattleMode.Training)
        {
            if (finalScore > previousBP)
                SaveResultToServer(finalScore);
            else
                ShowFinalResult(finalScore, false);
        }
        else if (mode == BattleMode.Arena)
            SaveResultToServer(finalScore);
        else
            ShowFinalResult(finalScore, false);
    }

    private void SaveResultToServer(int score)
    {
        if (NetworkManager.Instance == null)
            return;
        BattleResultRequest request = new BattleResultRequest
        {
            category_name = currentCategory.ToString(),
            total_score = score,
            results = battleRecords,
        };
        NetworkManager.Instance.SaveBattleResult(
            request,
            (res) => ShowFinalResult(score, true),
            (err) => ShowFinalResult(score, false)
        );
    }

    private void ShowFinalResult(int totalScore, bool isUpdated)
    {
        if (mode == BattleMode.Experience)
        {
            int correct = battleRecords.Count(r => r.is_correct);
            resultUI?.Show(correct, ExperienceSession.CurrentQuestionCount, totalScore);
        }
        else if (mode == BattleMode.Arena)
        {
            arenaResultUI?.Show(opponentLife <= 0, 1000, 1000, 10, "티어", null);
            arenaResultUI?.gameObject.SetActive(true);
        }
        else
            trainingResultUI?.Show(totalScore, isUpdated ? "전투력이 업데이트 되었습니다!" : "");
    }

    private void UpdateLifeUI()
    {
        if (mode == BattleMode.Experience)
            return;
        if (lifeText != null)
            lifeText.text = ExperienceSession.CurrentLife + " / " + ExperienceSession.MaxLife;
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
                heartIcons[i].sprite =
                    (i < ExperienceSession.CurrentLife) ? activeHeartSprite : inactiveHeartSprite;
        }
        if (mode == BattleMode.Arena)
        {
            for (int i = 0; i < opponentHeartIcons.Length; i++)
            {
                if (opponentHeartIcons[i] != null)
                    opponentHeartIcons[i].sprite =
                        (i < opponentLife)
                            ? opponentActiveHeartSprite
                            : opponentInactiveHeartSprite;
            }
        }
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

    private ExperienceDifficulty ParseDifficulty(string diffName)
    {
        if (string.IsNullOrEmpty(diffName))
            return ExperienceDifficulty.VeryEasy;
        string n = diffName.Replace(" ", "").ToUpper();
        return n switch
        {
            "VERYEASY" => ExperienceDifficulty.VeryEasy,
            "EASY" => ExperienceDifficulty.Easy,
            "HARD" => ExperienceDifficulty.Hard,
            "VERYHARD" => ExperienceDifficulty.VeryHard,
            "TOUGH" => ExperienceDifficulty.Tough,
            "VERYTOUGH" => ExperienceDifficulty.VeryTough,
            _ => ExperienceDifficulty.VeryEasy,
        };
    }

    private int GetDifficultyScore(ExperienceDifficulty diff)
    {
        return diff switch
        {
            ExperienceDifficulty.VeryEasy => 5,
            ExperienceDifficulty.Easy => 10,
            ExperienceDifficulty.Hard => 15,
            ExperienceDifficulty.VeryHard => 20,
            ExperienceDifficulty.Tough => 25,
            ExperienceDifficulty.VeryTough => 30,
            _ => 0,
        };
    }

    private void HookAllButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            choiceButtons[index].onClick.RemoveAllListeners();
            choiceButtons[index]
                .onClick.AddListener(() => ProcessAnswer(index.ToString() == correctAnswerText));
        }
        submitButton?.onClick.RemoveAllListeners();
        submitButton?.onClick.AddListener(() =>
            ProcessAnswer(inputField.text == correctAnswerText)
        );
        designSubmitButton?.onClick.RemoveAllListeners();
        designSubmitButton?.onClick.AddListener(() =>
            ProcessAnswer(designUserSequence.SequenceEqual(designCorrectSequence))
        );
        for (int i = 0; i < designChoiceButtons.Length; i++)
        {
            int index = i;
            designChoiceButtons[index].onClick.RemoveAllListeners();
            designChoiceButtons[index]
                .onClick.AddListener(() =>
                {
                    designUserSequence.Add(index);
                    UpdateDesignUI();
                });
        }
    }
}
