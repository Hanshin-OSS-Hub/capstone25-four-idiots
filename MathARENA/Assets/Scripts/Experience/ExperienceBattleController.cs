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
    private ExperienceDifficultyBar difficultyBar; // 난이도 아이콘용

    [SerializeField]
    private ExperienceTimer battleTimer; // [추가] 타이머 전용 칸

    [Header("Question UI")]
    [SerializeField]
    private TMP_Text questionCounterText;

    [SerializeField]
    private TMP_Text playerCurrentScoreText; // 플레이어 아래 점수 텍스트

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
    private TMP_Text inputField;

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

    [Header("Loading UI")]
    [SerializeField]
    private GameObject loadingOverlay; // [추가] 이 줄이 있어야 에러가 사라집니다.

    [Header("Formula UI")]
    [SerializeField]
    private RawImage questionFormulaImage; // 지문 수식용 [cite: 2026-05-05]

    [SerializeField]
    private List<RawImage> choiceFormulaImages; // 보기 수식용 (4개) [cite: 2026-05-05]

    // --- 내부 상태 변수 (140번 줄 근처로 통합) ---
    private List<QuestionResultData> battleRecords = new List<QuestionResultData>();
    private List<QuestionResultData> opponentRecords = new List<QuestionResultData>();
    private List<string> solvedQuestionIds = new List<string>();

    private float questionStartTime; // 144번 줄 (중복 제거됨)
    private string currentQuestionId; // 145번 줄 (중복 제거됨)
    private string currentSessionId; // 세션 관리용 (통합) [cite: 2026-05-08]

    private string currentDiffName = "VERY EASY";
    private int opponentLife = 4;
    private int currentOpponentRecordIndex = 0;

    private ExperienceCategory currentCategory;
    private string correctAnswerText = "";
    private int totalQuestionLimit = 30;

    private bool isProcessingAnswer = false; // 155번 줄 (중복 제거됨)

    // --- [실시간 아레나용 플래그 추가] ---
    private bool isArenaTickTriggered = false;

    // --- [연타 방지용 변수 추가] ---
    private float buttonEnableTime = 0f;

    private List<int> designUserSequence = new List<int>();
    private int[] designCorrectSequence;
    private string[] designOriginalTexts;

    private int currentOpponentPower; // 상대방의 실제 전투력 저장용

    // ExperienceBattleController.cs [160번 줄 근처]
    private List<ServerQuestionData> arenaQuestionList = new List<ServerQuestionData>(); // 아레나용 문제 저장소

    private string currentMatchId; // 아레나 매치 ID
    private List<OpponentRecord> currentOpponentRecords = new List<OpponentRecord>(); // 상대방의 문제별 기록

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
        isProcessingAnswer = false;

        if (userLifePanel != null)
            userLifePanel.SetActive(mode != BattleMode.Experience);
        if (opponentLifePanel != null)
            opponentLifePanel.SetActive(mode == BattleMode.Arena);

        UpdateLifeUI();

        if (mode == BattleMode.Arena)
        {
            ExperienceSession.TotalExpScore = GetCurrentCategoryCP();
        }
        else
        {
            ExperienceSession.TotalExpScore = 0; // 훈련장과 체험장은 무조건 0점부터 합산
        }

        ExperienceSession.CurrentQuestionCount = 0;
        currentOpponentRecordIndex = 0;
        currentSessionId = "";

        battleRecords.Clear();
        solvedQuestionIds.Clear();
        arenaQuestionList.Clear();

        // 점수판 UI를 현재 BP로 즉시 갱신
        UpdatePlayerScoreUI();

        SetupModeByCategory();

        if (mode == BattleMode.Arena)
        {
            opponentLife = 4;
            UpdateLifeUI();
            LoadArenaOpponentData();
        }
        else
        {
            SetupQuestionByCategory();
        }
    }

    // 점수 UI를 갱신하는 공용 함수
    private void UpdatePlayerScoreUI()
    {
        if (playerCurrentScoreText == null)
            return;

        // --- [수정 완료] 기획 명세서에 맞게 훈련장에서도 실시간으로 합산된 전투력(BP)을 표시합니다. ---
        // 기존의 "?? BP" 은폐 로직을 제거하고, 모든 모드(체험장, 훈련장, 아레나)에서 현재 점수를 실시간 노출합니다.
        playerCurrentScoreText.text = $"{ExperienceSession.TotalExpScore} BP";
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

    // --- [🔥 수정: 아레나/훈련장 세션 데이터와 완벽 연동되는 UI 패널 스위칭 함수] ---
    private void SetupModeByCategory()
    {
        // 현재 배틀 모드에 맞춰 알맞은 세션의 카테고리를 추려냅니다.
        ExperienceCategory cat = ExperienceSession.CurrentCategory;

        if (mode == BattleMode.Arena)
        {
            // 아레나 세션의 카테고리(정수형)를 ExperienceCategory 형변환하여 매핑
            cat = (ExperienceCategory)ArenaSession.CurrentCategory;
        }
        else if (mode == BattleMode.Training)
        {
            // 만약 TrainingSession.CurrentCategory가 별도로 있다면 그것과 매핑,
            // 현재 구조상 ExperienceSession을 공유 중이라면 그대로 유지됩니다.
            cat = ExperienceSession.CurrentCategory;
        }

        Debug.Log($"<color=orange>[패널 매칭] 현재 배틀 모드: {mode} / 인식된 종목: {cat}</color>");

        if (panelChoices != null)
            panelChoices.SetActive(
                cat == ExperienceCategory.Concept || cat == ExperienceCategory.Idea
            );

        if (panelDesign != null)
            panelDesign.SetActive(cat == ExperienceCategory.Design);

        if (panelInput != null)
            panelInput.SetActive(
                cat == ExperienceCategory.Calc || cat == ExperienceCategory.Practice
            );
    }

    private void LoadArenaOpponentData()
    {
        // ── ArenaMatchingUI에서 ArenaSession에 이미 저장해 둔 데이터를 그대로 사용 ──
        // 전투 씬에서 FindMatch를 다시 호출하면 매칭 씬에서 유저가 선택한 상대가
        // 아닌 서버의 첫 번째 candidate가 쓰이는 문제가 생긴다.

        currentMatchId = ArenaSession.MatchId;
        currentOpponentPower = ArenaSession.OpponentPower;
        currentOpponentRecords =
            ArenaSession.OpponentRecords
            ?? new System.Collections.Generic.List<MathArena.Network.OpponentRecord>();

        // 같은 상대와 재대결 시 마지막 문제 다음부터 시작 (명세서 §아레나 재대결 조항)
        int startIndex =
            ArenaSession.LastQuestionOrder > 0 ? ArenaSession.LastQuestionOrder + 1 : 0;
        ExperienceSession.CurrentQuestionCount = startIndex;
        currentOpponentRecordIndex = startIndex;

        Debug.Log(
            $"<color=orange>[아레나]</color> 상대: {ArenaSession.OpponentId} "
                + $"| matchId: {currentMatchId} "
                + $"| 상대기록 {currentOpponentRecords.Count}개 "
                + $"| {startIndex + 1}번 문제부터 시작"
        );

        // 상대 기록 상세 로그
        foreach (var r in currentOpponentRecords)
            Debug.Log(
                $"  → Q{r.question_order_number}: 정답={r.is_correct}, 시간={r.solve_time_sec}s"
            );

        SetupQuestionByCategory();
    }

    // ExperienceBattleController.cs의 SetupQuestionByCategory 메서드 수정
    private void SetupQuestionByCategory()
    {
        if (loadingOverlay != null)
            loadingOverlay.SetActive(true);

        Debug.Log("<color=white><b>[1단계] SetupQuestionByCategory 실행됨</b></color>");

        // --- [🔥 수정: 서버 통신 중 레이스 컨디션 차단 자물쇠 선제 잠금] ---
        isProcessingAnswer = true;
        isArenaTickTriggered = true; // 서버 응답이 와서 ApplyServerDataToUI가 열리기 전까지 Update 작동 강제 동결

        string cat = currentCategory.ToString().ToLower();
        currentDiffName = DetermineDifficulty();

        if (mode == BattleMode.Training)
        {
            if (!string.IsNullOrEmpty(currentSessionId))
            {
                RequestTrainingQuestion();
                return;
            }

            // --- [훈련장] 세션 생성 및 문제 상세 요청 ---
            NetworkManager.Instance.StartTraining(
                cat,
                currentDiffName,
                "",
                (res) =>
                {
                    if (res.success && res.data != null)
                    {
                        currentSessionId = res.data.session_id;
                        Debug.Log(
                            $"<color=cyan>[훈련장] 세션 생성 성공: {currentSessionId}</color>"
                        );

                        NetworkManager.Instance.GetTrainingQuestion(
                            currentSessionId,
                            (questionRes) =>
                            {
                                if (questionRes.success && questionRes.data != null)
                                {
                                    Debug.Log(
                                        "<color=lime>[훈련장] 문제 수신 완료 -> UI 적용</color>"
                                    );
                                    ApplyServerDataToUI(questionRes.data);
                                }
                                else
                                    HandleNetworkError("문제 데이터가 비어있습니다.");
                            },
                            (err) => HandleNetworkError(err)
                        );
                    }
                    else
                        HandleNetworkError("훈련장 세션 생성 실패");
                },
                (err) => HandleNetworkError(err)
            );
        }
        else if (mode == BattleMode.Arena)
        {
            // 이미 리스트를 받아놓은 상태라면 서버에 또 묻지 않고 리스트에서 가져옵니다.
            if (
                arenaQuestionList != null
                && arenaQuestionList.Count > ExperienceSession.CurrentQuestionCount
            )
            {
                // 리스트에서 꺼낼 때도 안전하게 다시 플래그 정비 후 바인딩
                isArenaTickTriggered = false;
                ApplyServerDataToUI(arenaQuestionList[ExperienceSession.CurrentQuestionCount]);
                return;
            }

            NetworkManager.Instance.StartMatch(
                cat,
                currentDiffName,
                currentMatchId,
                (res) =>
                {
                    if (res.success && res.data != null && res.data.questions != null)
                    {
                        arenaQuestionList = res.data.questions;
                        Debug.Log(
                            $"<color=orange>[아레나] {arenaQuestionList.Count}개의 문제를 수신했습니다.</color>"
                        );

                        if (ExperienceSession.CurrentQuestionCount >= arenaQuestionList.Count)
                        {
                            Debug.LogWarning(
                                $"[아레나 경고] 시작 인덱스({ExperienceSession.CurrentQuestionCount})가 수신된 문제 수({arenaQuestionList.Count})를 초과하여 0번 문제부터 시작합니다."
                            );
                            ExperienceSession.CurrentQuestionCount = 0;
                        }

                        ApplyServerDataToUI(
                            arenaQuestionList[ExperienceSession.CurrentQuestionCount]
                        );
                    }
                    else
                        HandleNetworkError("아레나 데이터를 가져오지 못했습니다.");
                },
                (err) => HandleNetworkError(err)
            );
        }
        else
        {
            // --- [체험장] 세션 생성 및 문제 상세 요청 ---
            if (!string.IsNullOrEmpty(currentSessionId))
            {
                RequestExperienceQuestion();
                return;
            }

            NetworkManager.Instance.StartExperience(
                cat,
                currentDiffName,
                "",
                (res) =>
                {
                    if (res.success && res.data != null)
                    {
                        currentSessionId = res.data.session_id;
                        Debug.Log(
                            $"<color=cyan>[체험장] 세션 생성 성공: {currentSessionId}</color>"
                        );

                        NetworkManager.Instance.GetExperienceQuestion(
                            currentSessionId,
                            (questionRes) =>
                            {
                                if (questionRes.success && questionRes.data != null)
                                {
                                    Debug.Log(
                                        "<color=lime>[체험장] 문제 수신 완료 -> UI 적용</color>"
                                    );
                                    ApplyServerDataToUI(questionRes.data);
                                }
                                else
                                    HandleNetworkError("문제 데이터가 비어있습니다.");
                                { }
                            },
                            (err) => HandleNetworkError(err)
                        );
                    }
                    else
                        HandleNetworkError("체험장 세션 생성 실패");
                },
                (err) => HandleNetworkError(err)
            );
        }
    }

    // 공통 에러 처리 함수
    private void RequestTrainingQuestion()
    {
        NetworkManager.Instance.GetTrainingQuestion(
            currentSessionId,
            (questionRes) =>
            {
                if (questionRes.success && questionRes.data != null)
                {
                    Debug.Log("<color=lime>[Training] Question received -> applying UI</color>");
                    ApplyServerDataToUI(questionRes.data);
                }
                else
                    HandleNetworkError("Question data is empty.");
            },
            (err) => HandleNetworkError(err)
        );
    }

    private void RequestExperienceQuestion()
    {
        NetworkManager.Instance.GetExperienceQuestion(
            currentSessionId,
            (questionRes) =>
            {
                if (questionRes.success && questionRes.data != null)
                {
                    Debug.Log("<color=lime>[Experience] Question received -> applying UI</color>");
                    ApplyServerDataToUI(questionRes.data);
                }
                else
                    HandleNetworkError("Question data is empty.");
            },
            (err) => HandleNetworkError(err)
        );
    }

    private void HandleNetworkError(string err)
    {
        Debug.LogError($"네트워크 에러: {err}");
        isProcessingAnswer = false;
        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);
    }

    private string DetermineDifficulty()
    {
        int qIdx = ExperienceSession.CurrentQuestionCount + 1;
        int cp = GetCurrentCategoryCP();

        // [기획서 반영] 체험장과 훈련장은 동일한 난이도 구성을 공유합니다.
        // 기존의 "체험장이면 무조건 VERY EASY / EASY 고정" 하드코딩 로직을 제거하고 통합합니다.

        // 1단계: 전투력 500 이상 (고급)
        if (cp >= 500)
        {
            if (qIdx <= 5)
                return "VERY EASY"; // 1 ~ 5번째
            if (qIdx <= 10)
                return "EASY"; // 6 ~ 10번째
            if (qIdx <= 15)
                return "HARD"; // 11 ~ 15번째
            if (qIdx <= 20)
                return "VERY HARD"; // 16 ~ 20번째
            if (qIdx <= 30)
                return "TOUGH"; // 21 ~ 30번째
            return "VERY TOUGH"; // 31번째 이후~
        }

        // 2단계: 전투력 200 이상 500 미만 (중급)
        if (cp >= 200)
        {
            if (qIdx <= 10)
                return "VERY EASY"; // 1 ~ 10번째
            if (qIdx <= 20)
                return "EASY"; // 11 ~ 20번째
            if (qIdx <= 30)
                return "HARD"; // 21 ~ 30번째
            return "VERY HARD"; // 31번째 이후~
        }

        // 3단계: 전투력 200 미만 기본 상태 (초급)
        return qIdx <= 20 ? "VERY EASY" : "EASY"; // 1~20번째 VERY EASY, 21번째~ EASY
    }

    // ExperienceBattleController.cs의 ApplyServerDataToUI 함수 전문 (334번 줄 근처)
    private void ApplyServerDataToUI(ServerQuestionData data)
    {
        Debug.Log("<color=yellow><b>[2단계] ApplyServerDataToUI 진입 성공!</b></color>");

        if (this == null || !gameObject.activeInHierarchy || data == null)
        {
            Debug.LogWarning("[경고] 스크립트가 꺼져있거나 데이터가 null입니다.");
            return;
        }

        // 1. 시간 및 세션 정보 초기화
        questionStartTime = Time.time;
        isArenaTickTriggered = false;
        if (!string.IsNullOrEmpty(data.session_id))
            currentSessionId = data.session_id;
        currentQuestionId = !string.IsNullOrEmpty(data.question_id) ? data.question_id : data.q_id;

        // --- [🔥 수정: 이전 판의 카테고리 잔상이 남지 않도록 세션 데이터 기준으로 완벽 동기화] ---
        if (mode == BattleMode.Arena)
        {
            currentCategory = (ExperienceCategory)ArenaSession.CurrentCategory;
        }
        else
        {
            currentCategory = ExperienceSession.CurrentCategory;
        }

        SetupModeByCategory();

        // [핵심] 타이머(Panel_Count) 리셋 명령
        if (battleTimer != null)
        {
            Debug.Log("<color=magenta>[타이머] battleTimer.ResetTimer(60f) 호출합니다.</color>");
            battleTimer.ResetTimer(60f);
        }

        // 난이도 아이콘 적용 (Panel_Difficulty)
        if (difficultyBar != null)
        {
            difficultyBar.ApplyDifficulty(ParseDifficulty(currentDiffName));
        }

        // 2. 정답 추출 로직
        if (currentCategory == ExperienceCategory.Design)
        {
            correctAnswerText = data.correct_answer ?? "";
            if (!string.IsNullOrEmpty(correctAnswerText))
            {
                string[] parts = correctAnswerText.Split('-');
                List<int> pSeq = new List<int>();
                foreach (string p in parts)
                    if (int.TryParse(p, out int v))
                        pSeq.Add(v - 1);
                designCorrectSequence = pSeq.ToArray();
            }
        }
        else
        {
            correctAnswerText = data.correct_answer?.Replace("$", "") ?? data.answer_val.ToString();
        }

        // [추가] 정답 디버그 로그 - 이제 콘솔에서 정답을 미리 확인할 수 있습니다.
        Debug.Log($"<color=lime><b>[정답 확인] {correctAnswerText}</b></color>");

        // 3. 지문 텍스트 적용
        if (questionText != null)
        {
            string content = !string.IsNullOrEmpty(data.content) ? data.content : data.text;
            questionText.text = content.Replace("$", "");
        }

        // 4. 패널 스위칭 및 데이터 세팅 (기존 로직 유지)
        bool isDesign = (currentCategory == ExperienceCategory.Design);
        bool isOCR = (
            currentCategory == ExperienceCategory.Calc
            || currentCategory == ExperienceCategory.Practice
        );
        if (panelChoices != null)
            panelChoices.SetActive(!isDesign && !isOCR);
        if (panelDesign != null)
            panelDesign.SetActive(isDesign);
        if (panelInput != null)
            panelInput.SetActive(isOCR);

        if (isOCR)
        {
            if (inputField != null)
                inputField.text = "입력된 숫자 : ";
            drawingCanvas?.ClearCanvas();
        }
        else if (isDesign)
        {
            designUserSequence.Clear();
            if (data.choices != null)
            {
                designOriginalTexts = data.choices.Select(c => c.Replace("$", "")).ToArray();
                for (int i = 0; i < designChoiceButtons.Length; i++)
                {
                    if (i < data.choices.Count)
                    {
                        designChoiceTexts[i].text = designOriginalTexts[i];
                        designChoiceButtons[i].gameObject.SetActive(true);
                    }
                    else
                        designChoiceButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < choiceTexts.Length; i++)
            {
                if (data.choices != null && i < data.choices.Count)
                {
                    choiceTexts[i].text = data.choices[i].Replace("$", "");
                    choiceButtons[i].gameObject.SetActive(true);
                }
                else
                    choiceButtons[i].gameObject.SetActive(false);
            }
        }

        UpdateQuestionCounterUI();
        //HookAllButtons();

        // [추가] 다음 문제를 위해 버튼들 다시 활성화
        foreach (var btn in choiceButtons)
        {
            if (btn != null)
                btn.interactable = true;
        }

        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);

        isProcessingAnswer = false;

        // --- [수정: 문제가 다 가동된 시점부터 1초간 연타 방지 쿨타임 적용] ---
        buttonEnableTime = Time.time + 1.0f; // 1.0초를 원하는 쿨타임(예: 0.5f 등)으로 조절 가능합니다.

        Debug.Log("<color=white>[완료] 모든 데이터 적용 완료.</color>");
    }

    private void Update()
    {
        // [수정] loadingOverlay가 켜져있거나(서버 통신 중), 처리 중일 때는 실시간 판정을 원천 중지합니다.
        if (loadingOverlay != null && loadingOverlay.activeInHierarchy)
            return;

        // 아레나 모드이고, 현재 문제가 활성화 상태이며, 아직 실시간 판정이 발동 안 했을 때만 수행
        if (mode == BattleMode.Arena && !isProcessingAnswer && !isArenaTickTriggered)
        {
            int currentQNum = ExperienceSession.CurrentQuestionCount + 1;
            OpponentRecord opponentData = null;

            if (currentOpponentRecords != null && currentOpponentRecords.Count > 0)
            {
                int totalOpponentRecords = currentOpponentRecords.Count;
                int virtualListIndex = (currentQNum - 1) % totalOpponentRecords;

                opponentData = currentOpponentRecords[virtualListIndex];
            }

            if (opponentData != null)
            {
                float elapsed = Time.time - questionStartTime;
                float targetTime = Mathf.Max(0.1f, opponentData.solve_time_sec);

                if (elapsed >= targetTime)
                {
                    isArenaTickTriggered = true; // 중복 실행 방지

                    if (opponentData.is_correct)
                    {
                        Debug.Log(
                            $"<color=red>[아레나 실시간]</color> 제 {currentQNum}문 (상대 인덱스 {currentOpponentRecords.IndexOf(opponentData)}번 기록 순환): 플레이어 피격"
                        );
                        ExecuteResultSequence(false, "PLAYER_TOO_SLOW");
                    }
                    else
                    {
                        Debug.Log(
                            $"<color=green>[아레나 실시간]</color> 제 {currentQNum}문 (상대 인덱스 {currentOpponentRecords.IndexOf(opponentData)}번 기록 순환): 상대 선 틀림 -> 상대방 피격"
                        );
                        ExecuteResultSequence(false, "OPPONENT_FAILED_FIRST");
                    }
                }
            }
        }
    }

    // --- [추가] 라텍스 이미지를 불러오는 코루틴 함수 --- [cite: 2026-05-05]
    private IEnumerator LoadLatexImage(string latex, RawImage targetImage)
    {
        // API 주소 (배경 투명화 옵션 포함)
        string apiUrl = "https://latex.codecogs.com/png.latex?\\dpi{150}\\bg_transparent";
        string encodedLatex = UnityEngine.Networking.UnityWebRequest.EscapeURL(latex);

        using (
            UnityEngine.Networking.UnityWebRequest request =
                UnityEngine.Networking.UnityWebRequestTexture.GetTexture(apiUrl + encodedLatex)
        )
        {
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
                targetImage.texture = tex;
                targetImage.SetNativeSize(); // 수식 비율에 맞게 크기 조절
            }
        }
    }

    // 기존: public void OnTimeOut() => ProcessAnswer(false);
    // 수정: 하트 차감과 OX 연출이 포함된 통합 함수를 호출합니다.
    public void OnTimeOut() => ExecuteResultSequence(false, "TIMEOUT");

    private void ContinueBattleProcess()
    {
        ExperienceSession.CurrentQuestionCount++;

        if (mode == BattleMode.Arena)
        {
            // 변수를 실제로 사용하는 로직을 추가하여 경고 해결
            // 사용했으므로 인덱스 증가
            currentOpponentRecordIndex++;
        }

        // 게임 종료 조건 체크
        bool isGameOver = ExperienceSession.CurrentQuestionCount >= totalQuestionLimit;

        if (isGameOver)
        {
            FinishBattle();
        }
        else
        {
            UpdateQuestionCounterUI();
            // [중요] 다음 문제를 서버에서 새로 받아와야 합니다.
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

        // 훈련장/체험장은 기존처럼 제출(submit/finish) 로직 유지
        if (mode == BattleMode.Arena)
        {
            // [수정] 아레나는 이미 답을 다 냈으므로, '종료' API만 호출하면 됩니다.
            Debug.Log(
                $"<color=orange>[아레나 종료]</color> Match ID: {currentMatchId} 종료 요청을 보냅니다."
            );

            NetworkManager.Instance.FinishMatch(
                currentMatchId,
                (res) =>
                {
                    Debug.Log("<color=green>[아레나] 최종 종료 및 결과 수신 성공</color>");
                    // 서버 응답에 따라 승패 연출 및 결과창 표시
                    ShowFinalResult(score, true);
                },
                (err) =>
                {
                    Debug.LogError($"[아레나] 종료 실패: {err}");
                    ShowFinalResult(score, false);
                }
            );
        }
        else
        {
            // 훈련장 및 체험장 기존 로직 (updated_cp 또는 total_power 사용)
            BattleResultRequest request = new BattleResultRequest
            {
                session_id = currentSessionId,
                category_name = currentCategory.ToString().ToLower(),
                total_power = score,
                results = battleRecords,
            };

            if (mode == BattleMode.Training)
            {
                NetworkManager.Instance.FinishTraining(
                    request,
                    (res) => ShowFinalResult(score, true),
                    (err) => ShowFinalResult(score, false)
                );
            }
            else
            {
                NetworkManager.Instance.SubmitExperience(
                    request,
                    (res) => ShowFinalResult(score, true),
                    (err) => ShowFinalResult(score, false)
                );
            }
        }
    }

    private void ShowFinalResult(int totalScore, bool isUpdated)
    {
        if (this == null)
            return;

        // [1] 체험장 모드: 맞힌 개수와 최종 점수 표시 (BP 단위)
        if (mode == BattleMode.Experience)
        {
            int correct = battleRecords.Count(r => r.is_correct);
            if (resultUI != null)
            {
                resultUI.Show(correct, ExperienceSession.CurrentQuestionCount, totalScore);
            }
        }
        // [2] 아레나 모드: 명세서 기반 레이팅 가감 및 100AR 승급 로직
        // [2] 아레나 모드: 명세서 기반 레이팅 가감 및 100AR 승급 로직
        else if (mode == BattleMode.Arena)
        {
            // 1. 전투력(BP) 차이(오차범위) 계산
            int myCP = GetCurrentCategoryCP();
            int opponentCP = currentOpponentPower;
            int diff = Mathf.Abs(myCP - opponentCP);

            // [수정] 내 하트도 없고 상대 하트도 없는 무승부 상황 방어 (내가 살고 상대가 죽어야만 승리)
            bool isWin = (opponentLife <= 0) && (ExperienceSession.CurrentLife > 0);
            int arChange = 0;

            // 2. 명세서 규칙 1~6번 완벽 매핑 (오차범위 경계값 오류 수정)
            if (isWin)
            {
                if (diff > 100)
                {
                    arChange = 15; // [규칙 2] 100 초과 상대 승리 -> +15
                }
                else if (diff >= 50 && diff <= 100)
                {
                    arChange = 10; // [규칙 1] 50 이상 100 이하 상대 승리 -> +10
                }
                else if (diff < 50)
                {
                    arChange = 5; // [규칙 3] 50 미만 상대 승리 -> +5
                }
            }
            else
            {
                if (diff > 100)
                {
                    arChange = -5; // [규칙 5] 100 초과 상대 패배 -> -5
                }
                else if (diff >= 50 && diff <= 100)
                {
                    arChange = -10; // [규칙 4] 50 이상 100 이하 상대 패배 -> -10
                }
                else if (diff < 50)
                {
                    arChange = -15; // [규칙 6] 50 미만 상대 패배 -> -15
                }
            }

            // 3. 누적 레이팅 계산 및 100AR 단위 승급 로직 (기존 유지)
            int cumulativeCurrentAR =
                (ExperienceSession.UserProfile != null)
                    ? ExperienceSession.UserProfile.arena_rating
                    : 0;
            int cumulativeNextAR = Mathf.Max(0, cumulativeCurrentAR + arChange);

            // UI 표시용 레이팅: 0~99점 표기용 (기존 유지)
            int displayCurrentAR = cumulativeCurrentAR % 100;
            int displayNextAR = cumulativeNextAR % 100;

            // 승급/강등 여부 판단 (기존 유지)
            bool isPromoted = (cumulativeNextAR / 100) > (cumulativeCurrentAR / 100);
            bool isDemoted = (cumulativeNextAR / 100) < (cumulativeCurrentAR / 100);

            // 4. 티어 정보 갱신 및 프로필 세션 반영 (기존 유지)
            var tierInfo = TierManager.GetTierInfo(cumulativeNextAR);

            if (ExperienceSession.UserProfile != null)
            {
                ExperienceSession.UserProfile.arena_rating = cumulativeNextAR;
                ExperienceSession.UserProfile.tier_name = tierInfo.fullName;
            }

            // 5. 아레나 결과창 출력 (기존 유지)
            if (arenaResultUI != null)
            {
                arenaResultUI.gameObject.SetActive(true);
                arenaResultUI.Show(
                    isWin,
                    displayCurrentAR,
                    displayNextAR,
                    arChange,
                    tierInfo.fullName,
                    Resources.Load<Sprite>($"Tiers/Tier_{tierInfo.tierIdx}_{tierInfo.gradeIdx}"),
                    isPromoted,
                    isDemoted
                );
            }

            Debug.Log(
                $"<color=yellow>[아레나 종료]</color> 승리:{isWin}, 변화량:{arChange}, 최종누적AR:{cumulativeNextAR}, 승급:{isPromoted}"
            );
        }
        // [3] 훈련장 모드: 최고 기록 경신 여부 표시
        else if (mode == BattleMode.Training)
        {
            if (trainingResultUI != null)
            {
                trainingResultUI.gameObject.SetActive(true);
                string updateMessage = isUpdated
                    ? "최고 기록을 경신했습니다! 프로필에 반영됩니다."
                    : "기존 최고 기록에 미달하여 갱신되지 않았습니다.";
                trainingResultUI.Show(totalScore, updateMessage);
            }
        }
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

    public void OnClickVerify()
    {
        if (drawingCanvas == null || ocrManager == null || inputField == null)
        {
            Debug.LogError("OCR 관련 컴포넌트가 연결되지 않았습니다!");
            return;
        }

        // 1. 캔버스에서 그림 가져오기
        Texture2D captured = drawingCanvas.GetCapturedTexture();

        // 2. AI에게 분석 요청
        int result = ocrManager.PredictDigit(captured);

        // 3. [수정 부분] 인식된 숫자를 "입력된 숫자 : 7" 형식으로 표시
        if (result != -1)
        {
            // $를 사용한 문자열 보간법이나 + 연산자를 사용합니다.
            inputField.text = $"입력된 숫자 : {result}";

            Debug.Log($"OCR 인식 완료: {result}");
        }
        else
        {
            inputField.text = "입력된 숫자 : 인식 실패";
        }
    }

    // ExperienceBattleController.cs 내의 OnClickSubmit 함수를 아래 내용으로 교체
    // ExperienceBattleController.cs 내의 OnClickSubmit 함수 수정본
    public void OnClickSubmit()
    {
        if (isProcessingAnswer)
            return;

        // [A] OCR 모드 (연산/실전) 처리
        if (
            currentCategory == ExperienceCategory.Calc
            || currentCategory == ExperienceCategory.Practice
        )
        {
            // 입력 필드에서 숫자만 추출
            string userDigits = new string(inputField.text.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(userDigits))
            {
                Debug.LogWarning("인식된 숫자가 없습니다. 다시 써주세요.");
                return;
            }

            // 1. 판정 결과 계산
            bool isCorrect = (userDigits == correctAnswerText.Trim());

            Debug.Log(
                $"[OCR 제출] 입력: {userDigits}, 정답: {correctAnswerText}, 결과: {isCorrect}"
            );

            // 2. 통합 판정 함수 실행 (하트 차감, OX 연출, 서버 전송 포함)
            ExecuteResultSequence(isCorrect, userDigits);

            // 3. UI 초기화 (연출 시작 시점에 즉시 초기화)
            drawingCanvas?.ClearCanvas();
            inputField.text = "입력된 숫자 : ";
        }
        // [B] 설계 모드 처리
        else if (currentCategory == ExperienceCategory.Design)
        {
            if (
                designCorrectSequence == null
                || designUserSequence.Count < designCorrectSequence.Length
            )
            {
                Debug.LogWarning("[설계 제출] 정답 데이터가 없거나 선택 개수가 부족합니다.");
                return;
            }

            // 1. 판정 결과 계산
            bool isAllCorrect = designUserSequence.SequenceEqual(designCorrectSequence);
            Debug.Log($"[설계 제출] 결과: {isAllCorrect}");

            // 2. 통합 판정 함수 실행 (하트 차감, OX 연출, 서버 전송 포함)
            ExecuteResultSequence(isAllCorrect, string.Join("", designUserSequence));
        }
    }

    private void SendRecordToServer(string userAnswer, bool isCorrect)
    {
        // 서버가 'question_id'와 'session_id'를 찾지 못하는 문제를 해결하기 위해
        // 데이터를 객체 규격에 맞춰 확실히 채워줍니다.
        var resultData = new QuestionResultData
        {
            question_id = currentQuestionId, // 필수 필드
            q_id = currentQuestionId, // 보조 필드
            solve_time_sec = Mathf.RoundToInt(Time.time - questionStartTime),
            answer = userAnswer,
            is_correct = isCorrect,
        };

        var request = new BattleResultRequest
        {
            session_id = currentSessionId, // 필수 필드
            category_name = currentCategory.ToString(),
            results = new List<QuestionResultData> { resultData },
        };

        // 기록 저장 시도 (로컬 판정은 이미 끝났으므로 실패해도 게임은 진행됨)
        NetworkManager.Instance.SubmitExperience(
            request,
            (res) => Debug.Log("[서버 기록] 저장 성공"),
            (err) => Debug.LogWarning($"[서버 기록] 저장 실패(무시): {err}")
        );
    }

    private void HookAllButtons()
    {
        // [보완] 모든 버튼의 리스너를 먼저 확실하게 제거
        if (submitButton != null)
            submitButton.onClick.RemoveAllListeners();
        if (designSubmitButton != null)
            designSubmitButton.onClick.RemoveAllListeners();

        // 1. OCR 제출 버튼
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnClickSubmitOCR);
        }

        // 2. 설계 제출 버튼
        if (designSubmitButton != null)
        {
            designSubmitButton.onClick.AddListener(OnClickSubmitDesign);
        }

        // 3. 객관식(Concept/Idea) 보기 버튼
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null)
                continue;
            int index = i;
            choiceButtons[index].onClick.RemoveAllListeners();
            // [중요] 람다식 대신 직접적인 호출 구조로 변경하여 중첩 최소화
            choiceButtons[index]
                .onClick.AddListener(() =>
                {
                    OnClickChoice(index);
                });
        }

        // 4. 설계 보기 버튼
        for (int i = 0; i < designChoiceButtons.Length; i++)
        {
            if (designChoiceButtons[i] == null)
                continue;
            int index = i;
            designChoiceButtons[index].onClick.RemoveAllListeners();
            designChoiceButtons[index].onClick.AddListener(() => OnClickDesignElement(index));
        }
    }

    // 2. [객관식] 클릭 처리
    private void OnClickChoice(int index)
    {
        // 1. 이미 처리 중이면 즉시 리턴
        if (isProcessingAnswer || isArenaTickTriggered || Time.time < buttonEnableTime)
            return;

        isProcessingAnswer = true; // 자물쇠 잠금

        // [핵심 추가] 모든 객관식 버튼을 즉시 비활성화 (물리적 차단)
        foreach (var btn in choiceButtons)
        {
            if (btn != null)
                btn.interactable = false;
        }

        string userChoiceText = choiceTexts[index].text.Trim();
        string serverAnswer = correctAnswerText.Trim();
        bool isCorrect = (index.ToString() == serverAnswer) || (userChoiceText == serverAnswer);

        Debug.Log($"[객관식 클릭] index: {index}, 결과: {isCorrect}");
        ExecuteResultSequence(isCorrect, userChoiceText);
    }

    // 3. [설계] 보기 클릭 처리 (선택/취소)
    private void OnClickDesignElement(int index)
    {
        // [수정] 여기서는 잠금을 걸면 안 됩니다. 제출 버튼을 누를 때까지 여러 번 클릭해야 하기 때문입니다.
        // if (isProcessingAnswer) return;
        // isProcessingAnswer = true; <-- 이 줄을 지우거나 주석 처리하세요.

        if (designUserSequence.Contains(index))
        {
            designUserSequence.Remove(index);
        }
        else
        {
            designUserSequence.Add(index);
        }

        RefreshDesignOrderLabels();
    }

    // 4. [설계] 최종 제출 버튼 클릭 처리 보완
    public void OnClickSubmitDesign()
    {
        // [수정] 실시간 타임아웃 트리거 발동 시 입력 원천 차단
        // 연타 방지 시간 체크 추가
        if (isProcessingAnswer || isArenaTickTriggered || Time.time < buttonEnableTime)
            return;

        if (designCorrectSequence == null)
            return;
        if (designUserSequence.Count < designCorrectSequence.Length)
            return;

        bool isAllCorrect = designUserSequence.SequenceEqual(designCorrectSequence);
        ExecuteResultSequence(isAllCorrect, string.Join("", designUserSequence));
    }

    // 5. [OCR] 제출 버튼 클릭 처리 보완
    public void OnClickSubmitOCR()
    {
        // [수정] 실시간 타임아웃 트리거 발동 시 입력 원천 차단
        if (isProcessingAnswer || isArenaTickTriggered || Time.time < buttonEnableTime)
            return;

        string userDigits = new string(inputField.text.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(userDigits))
            return;

        bool isCorrect = (userDigits == correctAnswerText.Trim());

        drawingCanvas?.ClearCanvas();
        inputField.text = "입력된 숫자 : ";

        ExecuteResultSequence(isCorrect, userDigits);
    }

    // 6. [공용] 판정 연출 및 다음 문제 전환 (OX가 뜨게 하는 핵심)
    // ExperienceBattleController.cs 내의 ExecuteResultSequence 함수
    // [중요] ExecuteResultSequence 부분만 교체하시면 됩니다.
    // 6. [공용] 판정 연출 및 다음 문제 전환 (OX가 뜨게 하는 핵심)
    private void ExecuteResultSequence(bool isPlayerCorrect, string answerForServer)
    {
        isProcessingAnswer = true;

        if (battleTimer != null)
        {
            battleTimer.StopTimer();
        }

        int solve_time_sec = Mathf.RoundToInt(Time.time - questionStartTime);

        // 문제 기록 저장용 데이터 생성
        var resultData = new QuestionResultData
        {
            question_id = currentQuestionId,
            solve_time_sec = solve_time_sec,
            answer = answerForServer,
            is_correct = isPlayerCorrect,
        };
        battleRecords.Add(resultData);

        // 서버 전송용 패킷 조립
        BattleResultRequest submitReq = new BattleResultRequest
        {
            session_id = (mode == BattleMode.Arena) ? "" : currentSessionId,
            match_id = (mode == BattleMode.Arena) ? currentMatchId : "",
            question_id = currentQuestionId,
            category_name = currentCategory.ToString().ToLower(),
            solve_time_sec = solve_time_sec,
            is_correct = isPlayerCorrect,
        };

        if (currentCategory == ExperienceCategory.Design)
            submitReq.answer_order = answerForServer;
        else
            submitReq.answer = answerForServer;

        // --- [실시간 공방 판정 규칙 로직] ---
        bool playerAttacks = false;
        bool nobodyAttacks = false;

        if (mode == BattleMode.Arena)
        {
            if (answerForServer == "OPPONENT_FAILED_FIRST")
            {
                // 1. 상대방이 먼저 틀린 기록 시간에 도달한 상황 -> 내가 상대를 때림
                playerAttacks = true;
            }
            else if (answerForServer == "PLAYER_TOO_SLOW")
            {
                // 2. 내가 제한 시간 내에 못 풀어서 상대가 날 때린 상황 -> 내가 맞음
                playerAttacks = false;
            }
            else
            {
                // 3. 내가 버튼을 직접 눌러서 판정이 일어난 경우
                if (!isPlayerCorrect)
                {
                    // 내가 풀었는데 틀렸다 -> 무조건 내가 피격
                    playerAttacks = false;
                }
                else
                {
                    int currentQNum = ExperienceSession.CurrentQuestionCount + 1;
                    var opponentData = currentOpponentRecords?.Find(r =>
                        r.question_order_number == currentQNum
                    );

                    if (opponentData != null && !opponentData.is_correct)
                    {
                        // 나는 맞췄는데 상대는 과거에 틀렸었다 -> 내가 상대를 때림
                        playerAttacks = true;
                    }
                    else
                    {
                        // 둘 다 맞춘 경우 -> 순수 속도 경쟁 (내가 더 빠르면 내가 공격)
                        int opTime = (opponentData != null) ? opponentData.solve_time_sec : 60;
                        playerAttacks = (solve_time_sec <= opTime);
                    }
                }
            }
        }
        else
        {
            playerAttacks = isPlayerCorrect;
        }

        // --- [서버 비동기 전송 및 콜백 연결] ---
        if (!solvedQuestionIds.Contains(currentQuestionId))
        {
            solvedQuestionIds.Add(currentQuestionId);
            isProcessingAnswer = true;

            if (mode == BattleMode.Arena)
            {
                NetworkManager.Instance.SubmitMatch(
                    submitReq,
                    (res) =>
                    {
                        Debug.Log("<color=green>[서버 응답 완료]</color> 아레나 문제 제출 성공.");
                        ProceedToSequenceAndNext(nobodyAttacks, playerAttacks);
                    },
                    (err) =>
                    {
                        Debug.LogWarning($"[서버 에러] 제출 실패({err}), 연출을 강제 진행합니다.");
                        ProceedToSequenceAndNext(nobodyAttacks, playerAttacks);
                    }
                );
            }
            else if (mode == BattleMode.Training)
            {
                NetworkManager.Instance.SubmitTraining(
                    submitReq,
                    (res) => ProceedToSequenceAndNext(nobodyAttacks, playerAttacks),
                    (err) => ProceedToSequenceAndNext(nobodyAttacks, playerAttacks)
                );
            }
            else
            {
                NetworkManager.Instance.SubmitExperience(
                    submitReq,
                    (res) => ProceedToSequenceAndNext(nobodyAttacks, playerAttacks),
                    (err) => ProceedToSequenceAndNext(nobodyAttacks, playerAttacks)
                );
            }
        }
        else
        {
            // 이미 풀었던 문제라면 대기 없이 바로 연출 진행
            ProceedToSequenceAndNext(nobodyAttacks, playerAttacks);
        }
    }

    // --- [새로 추가된 헬퍼 함수] 서버 응답이 확인된 후 안전하게 하트 깎고 연출 트는 함수 ---
    // --- [수정] 서버 응답 확인 후 하트 차감, 상세 로그 출력, 연출 트리거 함수 ---
    // --- [수정] 0으로 나누기 방어막이 추가된 헬퍼 함수 ---
    private void ProceedToSequenceAndNext(bool nobodyAttacks, bool playerAttacks)
    {
        // 1. 하트 차감 반영 (아레나 모드 등 라이프 기반인 경우)
        if (!nobodyAttacks)
        {
            if (playerAttacks)
                opponentLife--;
            else
                ExperienceSession.CurrentLife--;
        }
        UpdateLifeUI();

        int currentQNum = ExperienceSession.CurrentQuestionCount + 1;

        // --- 📊 [모드별 맞춤형 전광판 로그 시스템] ---
        if (mode == BattleMode.Training)
        {
            // [훈련장 전용 로그]
            string trainingState =
                nobodyAttacks ? "<color=yellow><b>[무승부]</b></color> 시간 초과 혹은 처리 불능"
                : playerAttacks ? "<color=lime><b>[정답!]</b></color> 문제를 맞췄습니다. 💥"
                : "<color=red><b>[오답]</b></color> 문제를 틀렸습니다. 💔";

            string diffName = currentDiffName;

            // [★ 수정 포인트]: 실제 전광판이 확정된 이 시점에 난이도별 점수를 안전하게 연산합니다.
            int earnedPoints =
                (playerAttacks && !nobodyAttacks)
                    ? GetDifficultyScore(ParseDifficulty(diffName))
                    : 0;

            // [★ 핵심 추가]: 로그를 찍기 직전에 세션 점수를 더하고 점수판 UI(?? BP 유지)를 동기화합니다.
            if (earnedPoints > 0)
            {
                ExperienceSession.TotalExpScore += earnedPoints;
                UpdatePlayerScoreUI();
            }

            string trainingLogBoard =
                $"\n========================================\n"
                + $"   <b>[훈련장 결과 판정] 제 {currentQNum} 문</b> (난이도: {diffName})\n"
                + $"----------------------------------------\n"
                + $" • <b>판정 결과 :</b> {trainingState}\n"
                + $" • <b>이번 획득 점수 :</b> <color=cyan>+{earnedPoints} BP</color>\n"
                + $" • <b>현재 누적 전투력 :</b> <color=yellow><b>{ExperienceSession.TotalExpScore} BP</b></color>\n"
                + $"========================================";
            Debug.Log(trainingLogBoard);
        }
        else
        {
            // [아레나 및 기본 모드 전용 로그]
            int totalOpponentRecords =
                (currentOpponentRecords != null) ? Mathf.Max(1, currentOpponentRecords.Count) : 1;
            int virtualListIndex = (currentQNum - 1) % totalOpponentRecords;

            int displayOpponentQNum =
                (currentOpponentRecords != null && currentOpponentRecords.Count > virtualListIndex)
                    ? currentOpponentRecords[virtualListIndex].question_order_number
                    : currentQNum;

            string battleState =
                nobodyAttacks ? "<color=yellow><b>[무승부/무공격]</b></color> 둘 다 오답"
                : playerAttacks ? "<color=lime><b>[플레이어 턴]</b></color> 상대방 피격! 💥"
                : "<color=red><b>[상대방 턴]</b></color> 플레이어 피격! 💔";

            string arenaLogBoard =
                $"\n========================================\n"
                + $"   <b>[아레나 결과 판정] 제 {currentQNum} 장</b> (상대 원본 {displayOpponentQNum}번 기록 매핑)\n"
                + $"----------------------------------------\n"
                + $" • <b>판정 결과 :</b> {battleState}\n"
                + $" • <b>내 남은 하트 :</b> {ExperienceSession.CurrentLife} / {ExperienceSession.MaxLife}\n"
                + $" • <b>상대 남은 하트 :</b> {opponentLife} / 4\n"
                + $"========================================";
            Debug.Log(arenaLogBoard);
        }
        // --------------------------------------------

        // 2. 연출 실행 후 다음 문제 혹은 종료 처리
        if (sequenceManager != null && sequenceManager.gameObject.activeInHierarchy)
        {
            sequenceManager.PlaySequence(nobodyAttacks ? false : playerAttacks);
            sequenceManager.OnSequenceComplete = () => CheckBattleEndCondition();
        }
        else
        {
            CheckBattleEndCondition();
        }
    }

    // 7. [설계] 순서 라벨 갱신
    private void RefreshDesignOrderLabels()
    {
        for (int i = 0; i < designChoiceTexts.Length; i++)
        {
            if (i < designOriginalTexts.Length)
            {
                // 1. 텍스트 초기화 (번호 없는 원본 텍스트)
                designChoiceTexts[i].text = designOriginalTexts[i];

                // 2. 투명도 초기화 (기본은 불투명)
                Color c = designChoiceButtons[i].image.color;
                c.a = 1.0f; // 100% 불투명
                designChoiceButtons[i].image.color = c;
            }
        }

        // 3. 선택된 버튼들만 다시 번호를 붙이고 투명하게 만듦
        for (int i = 0; i < designUserSequence.Count; i++)
        {
            int btnIdx = designUserSequence[i];
            if (btnIdx < designChoiceTexts.Length)
            {
                // 번호 부여 (1. , 2. ...)
                designChoiceTexts[btnIdx].text = $"{i + 1}. {designOriginalTexts[btnIdx]}";

                // [핵심] 투명도 조절 (0.4f ~ 0.5f 정도로 설정하면 희미해짐)
                Color c = designChoiceButtons[btnIdx].image.color;
                c.a = 0.5f; // 50% 투명도
                designChoiceButtons[btnIdx].image.color = c;
            }
        }
    }

    // --- 마지막 남은 에러 해결: 심판 함수 추가 ---
    private void CheckBattleEndCondition()
    {
        // 1. 연출이 끝났으므로 다음 입력을 받을 수 있게 잠금 해제
        isProcessingAnswer = false;

        // 2. 아레나 모드: 나 혹은 상대방의 생명이 0인지 확인
        if (mode == BattleMode.Arena)
        {
            if (ExperienceSession.CurrentLife <= 0 || opponentLife <= 0)
            {
                Debug.Log("[아레나] 전투 종료 조건 충족");
                FinishBattle();
                return;
            }
        }
        // 3. 훈련장 모드: 내 생명이 0인지 확인
        else if (mode == BattleMode.Training)
        {
            if (ExperienceSession.CurrentLife <= 0)
            {
                Debug.Log("[훈련장] 생명력 소진으로 종료");
                FinishBattle();
                return;
            }
        }

        ContinueBattleProcess();
    }
}
