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

        // [수정] 0점이 아닌, 현재 카테고리의 BP를 시작 점수로 설정합니다.
        ExperienceSession.TotalExpScore = GetCurrentCategoryCP();

        ExperienceSession.CurrentQuestionCount = 0;
        currentOpponentRecordIndex = 0;

        battleRecords.Clear();
        solvedQuestionIds.Clear();

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
        if (playerCurrentScoreText != null)
        {
            // 현재 내 BP에 획득한 점수가 실시간으로 합쳐져 보입니다.
            playerCurrentScoreText.text = $"{ExperienceSession.TotalExpScore} BP";
        }
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
        if (loadingOverlay != null)
            loadingOverlay.SetActive(true);

        // 아레나 매칭 시작 (서버에서 상대방의 기록 데이터를 가져옴)
        string cat = currentCategory.ToString().ToLower();

        NetworkManager.Instance.FindMatch(
            cat,
            (res) =>
            {
                if (res.success && res.data?.candidates?.Count > 0)
                {
                    var candidate = res.data.candidates[0];
                    currentMatchId = candidate.match_id;
                    currentOpponentPower = candidate.opponent.power;
                    currentOpponentRecords = candidate.opponent_records;

                    // [핵심] 명세서 38번: 마지막으로 푼 문제 번호 다음부터 시작
                    // 만약 5번까지 풀었다면(0~4), 5번 인덱스부터 시작합니다.
                    ExperienceSession.CurrentQuestionCount = candidate.last_question_order + 1;

                    // 상대 기록 인덱스도 동일하게 맞춰줍니다.
                    currentOpponentRecordIndex = candidate.last_question_order + 1;

                    Debug.Log(
                        $"<color=orange>[아레나]</color> {ExperienceSession.CurrentQuestionCount}번 문제부터 재개합니다."
                    );

                    if (loadingOverlay != null)
                        loadingOverlay.SetActive(false);
                    SetupQuestionByCategory();
                }
            },
            (err) => HandleNetworkError(err)
        );
    }

    // ExperienceBattleController.cs의 SetupQuestionByCategory 메서드 수정
    private void SetupQuestionByCategory()
    {
        if (loadingOverlay != null)
            loadingOverlay.SetActive(true);

        // [CCTV 1] 함수가 실행되는지 확인하는 로그
        Debug.Log("<color=white><b>[1단계] SetupQuestionByCategory 실행됨</b></color>");

        isProcessingAnswer = true;
        string cat = currentCategory.ToString().ToLower();
        currentDiffName = DetermineDifficulty();

        if (mode == BattleMode.Training)
        {
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
                ApplyServerDataToUI(arenaQuestionList[ExperienceSession.CurrentQuestionCount]);
                return;
            }

            // 처음 시작할 때만 서버에서 리스트를 가져옵니다.
            NetworkManager.Instance.StartMatch(
                cat,
                currentDiffName,
                currentMatchId,
                (res) =>
                {
                    if (res.success && res.data != null && res.data.questions != null)
                    {
                        arenaQuestionList = res.data.questions; // 리스트 전체 저장
                        Debug.Log(
                            $"<color=orange>[아레나] {arenaQuestionList.Count}개의 문제를 수신했습니다.</color>"
                        );

                        // 첫 번째 문제를 화면에 띄웁니다.
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
    private void HandleNetworkError(string err)
    {
        Debug.LogError($"네트워크 에러: {err}");
        isProcessingAnswer = false;
        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);
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
        if (!string.IsNullOrEmpty(data.session_id))
            currentSessionId = data.session_id;
        currentQuestionId = !string.IsNullOrEmpty(data.question_id) ? data.question_id : data.q_id;
        currentCategory = ExperienceSession.CurrentCategory;

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
        HookAllButtons();
        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);

        isProcessingAnswer = false;
        Debug.Log("<color=white>[완료] 모든 데이터 적용 완료.</color>");
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
        else if (mode == BattleMode.Arena)
        {
            // 1. 전투력(BP) 차이(오차범위) 계산
            int myCP = GetCurrentCategoryCP();
            int opponentCP = currentOpponentPower;
            int diff = Mathf.Abs(myCP - opponentCP);

            bool isWin = opponentLife <= 0;
            int arChange = 0;

            // 2. 명세서 51~56번: 오차범위별 AR 가감 수치 결정
            if (isWin)
            {
                if (diff > 100)
                    arChange = 15; // 100 초과 상대 승리
                else if (diff >= 50)
                    arChange = 10; // 50~100 상대 승리
                else
                    arChange = 5; // 50 미만 상대 승리
            }
            else
            {
                if (diff > 100)
                    arChange = -5; // 100 초과 상대 패배
                else if (diff >= 50)
                    arChange = -10; // 50~100 상대 패배
                else
                    arChange = -15; // 50 미만 상대 패배
            }

            // 3. 누적 레이팅 계산 및 100AR 단위 승급 로직
            int cumulativeCurrentAR =
                (ExperienceSession.UserProfile != null)
                    ? ExperienceSession.UserProfile.arena_rating
                    : 0;
            int cumulativeNextAR = Mathf.Max(0, cumulativeCurrentAR + arChange);

            // UI 표시용 레이팅: 100점을 넘으면 0부터 다시 시작되는 느낌 구현 (0~99점)
            int displayCurrentAR = cumulativeCurrentAR % 100;
            int displayNextAR = cumulativeNextAR % 100;

            // 승급 여부 판단: 100단위 숫자가 바뀌었는지 확인
            bool isPromoted = (cumulativeNextAR / 100) > (cumulativeCurrentAR / 100);

            // 4. 티어 정보 갱신 (TierManager 사용)
            var tierInfo = TierManager.GetTierInfo(cumulativeNextAR);

            // 로비 및 프로필 즉시 반영을 위해 세션 데이터 갱신
            if (ExperienceSession.UserProfile != null)
            {
                ExperienceSession.UserProfile.arena_rating = cumulativeNextAR;
                ExperienceSession.UserProfile.tier_name = tierInfo.fullName;
            }

            // 5. 아레나 결과창 출력
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
                    isPromoted // 승급 시 결과창에 "티어 승급!" 알림 표시
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
        // 1. [중요] OCR 전용 제출 버튼 연결
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnClickSubmitOCR);
            Debug.Log("[연결 완료] OCR 제출 버튼 -> OnClickSubmitOCR");
        }

        // 2. [중요] 설계 모드 전용 제출 버튼 연결
        if (designSubmitButton != null)
        {
            designSubmitButton.onClick.RemoveAllListeners();
            designSubmitButton.onClick.AddListener(OnClickSubmitDesign);
            Debug.Log("[연결 완료] 설계 제출 버튼 -> OnClickSubmitDesign");
        }

        // 3. 객관식(Concept/Idea) 보기 버튼 설정
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            if (choiceButtons[index] == null)
                continue;

            choiceButtons[index].onClick.RemoveAllListeners();
            choiceButtons[index].onClick.AddListener(() => OnClickChoice(index));
        }

        // 4. 설계 모드(Design) 보기 버튼 설정 (선택 및 취소 기능)
        for (int i = 0; i < designChoiceButtons.Length; i++)
        {
            int index = i;
            if (designChoiceButtons[index] == null)
                continue;

            designChoiceButtons[index].onClick.RemoveAllListeners();
            designChoiceButtons[index].onClick.AddListener(() => OnClickDesignElement(index));
        }
    }

    // 2. [객관식] 클릭 처리
    private void OnClickChoice(int index)
    {
        if (isProcessingAnswer)
            return;

        string userChoiceText = choiceTexts[index].text.Trim();
        string serverAnswer = correctAnswerText.Trim();
        bool isCorrect = (index.ToString() == serverAnswer) || (userChoiceText == serverAnswer);

        Debug.Log($"[객관식] 선택: {index}, 결과: {isCorrect}");
        ExecuteResultSequence(isCorrect, userChoiceText);
    }

    // 3. [설계] 보기 클릭 처리 (선택/취소)
    private void OnClickDesignElement(int index)
    {
        if (isProcessingAnswer)
            return;

        // 리스트에 이미 있으면 제거(취소), 없으면 추가(선택)
        if (designUserSequence.Contains(index))
        {
            designUserSequence.Remove(index);
        }
        else
        {
            designUserSequence.Add(index);
        }

        // 상태가 변했으니 UI(텍스트 + 투명도)를 다시 그림
        RefreshDesignOrderLabels();
    }

    // 4. [설계] 최종 제출 버튼 클릭
    public void OnClickSubmitDesign()
    {
        // 1. 버튼이 눌렸는지 확인하는 로그
        Debug.Log("[설계 제출] 버튼 클릭됨");

        if (isProcessingAnswer)
        {
            Debug.LogWarning("[설계 제출] 이미 처리 중이라 무시됩니다.");
            return;
        }

        if (designCorrectSequence == null)
        {
            // [원인 가능성 1] 서버에서 answer_order를 제대로 못 받아온 경우
            Debug.LogError(
                "[설계 제출] 서버에서 정답 데이터(designCorrectSequence)를 받지 못했습니다!"
            );
            return;
        }

        if (designUserSequence.Count < designCorrectSequence.Length)
        {
            // [원인 가능성 2] 보기 개수를 다 채우지 않은 경우
            Debug.LogWarning(
                $"[설계 제출] 개수 부족: {designUserSequence.Count} / {designCorrectSequence.Length}"
            );
            return;
        }

        bool isAllCorrect = designUserSequence.SequenceEqual(designCorrectSequence);
        Debug.Log($"[설계 판정] 결과: {isAllCorrect}");

        ExecuteResultSequence(isAllCorrect, string.Join("", designUserSequence));
    }

    // 5. [OCR] 제출 버튼 클릭
    public void OnClickSubmitOCR()
    {
        if (isProcessingAnswer)
            return;

        string userDigits = new string(inputField.text.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(userDigits))
            return;

        bool isCorrect = (userDigits == correctAnswerText.Trim());
        Debug.Log($"[OCR 제출] 입력: {userDigits}, 결과: {isCorrect}");

        drawingCanvas?.ClearCanvas();
        inputField.text = "입력된 숫자 : ";

        ExecuteResultSequence(isCorrect, userDigits);
    }

    // 6. [공용] 판정 연출 및 다음 문제 전환 (OX가 뜨게 하는 핵심)
    // ExperienceBattleController.cs 내의 ExecuteResultSequence 함수
    // [중요] ExecuteResultSequence 부분만 교체하시면 됩니다.
    private void ExecuteResultSequence(bool isPlayerCorrect, string answerForServer)
    {
        // [1] 중복 실행 방지 (타임아웃은 예외)
        if (isProcessingAnswer && answerForServer != "TIMEOUT")
            return;

        // [2] 타이머 정지
        if (battleTimer != null)
        {
            battleTimer.StopTimer();
        }

        isProcessingAnswer = true;

        // 사용자의 풀이 시간을 초 단위로 계산 [통일: solve_time_sec]
        int solve_time_sec = Mathf.RoundToInt(Time.time - questionStartTime);

        // [3] 점수 가산 (훈련장 및 아레나 공통)
        if (isPlayerCorrect)
        {
            int earned = GetDifficultyScore(ParseDifficulty(currentDiffName));
            ExperienceSession.TotalExpScore += earned;
            UpdatePlayerScoreUI();
        }

        // 결과 데이터 생성
        var resultData = new QuestionResultData
        {
            question_id = currentQuestionId,
            solve_time_sec = solve_time_sec,
            answer = answerForServer,
            is_correct = isPlayerCorrect,
        };
        battleRecords.Add(resultData);

        // [4] 서버 제출 (모드별 분기)
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

        if (mode == BattleMode.Arena)
            NetworkManager.Instance.SubmitMatch(submitReq, null, null);
        else if (mode == BattleMode.Training)
            NetworkManager.Instance.SubmitTraining(submitReq, null, null);
        else
            NetworkManager.Instance.SubmitExperience(submitReq, null, null);

        // [5] 아레나 승패 판정 로직 (명세서 39~42번 기준)
        bool playerAttacks = false;

        if (mode == BattleMode.Arena)
        {
            int currentQNum = ExperienceSession.CurrentQuestionCount + 1;

            // [확인용 로그] 현재 리스트에 데이터가 몇 개나 있는지 먼저 찍어봅니다.
            Debug.Log(
                $"<color=white>[아레나 체크]</color> 현재 리스트 내 기록 개수: {currentOpponentRecords?.Count ?? 0}개"
            );

            var opponentData = currentOpponentRecords.Find(r =>
                r.question_order_number == currentQNum
            );

            if (opponentData != null)
            {
                // [로그 추가] 상대방의 정답 여부와 풀이 시간을 실시간으로 출력합니다.
                Debug.Log(
                    $"<color=cyan>[아레나 대조]</color> {currentQNum}번 문제 "
                        + $"| 나: {(isPlayerCorrect ? "O" : "X")} ({solve_time_sec}초) "
                        + $"| 상대: {(opponentData.is_correct ? "O" : "X")} ({opponentData.solve_time_sec}초)"
                );

                if (!isPlayerCorrect)
                {
                    playerAttacks = false;
                }
                else if (isPlayerCorrect && !opponentData.is_correct)
                {
                    playerAttacks = true;
                }
                else if (isPlayerCorrect && opponentData.is_correct)
                {
                    playerAttacks = (solve_time_sec < opponentData.solve_time_sec);
                }
            }
            else
            {
                // 상대 기록이 없을 경우 일반 판정 (맞추면 공격)
                // [중요] 봇이거나 기록이 없을 때 찍히는 로그
                Debug.LogWarning(
                    $"<color=red>[아레나 경고]</color> {currentQNum}번 문제에 대한 상대방 기록이 없습니다. (봇 판정)"
                );
                playerAttacks = isPlayerCorrect;
            }
        }
        else
        {
            playerAttacks = isPlayerCorrect;
        }

        // [6] 체력 차감 및 UI 업데이트
        if (playerAttacks)
        {
            opponentLife--; // 사용자가 공격 성공
        }
        else
        {
            ExperienceSession.CurrentLife--; // 상대방이 공격 성공
        }
        UpdateLifeUI();

        // [7] 애니메이션 및 시퀀스 실행
        if (sequenceManager != null)
        {
            sequenceManager.PlaySequence(playerAttacks);
            sequenceManager.OnSequenceComplete = () => CheckBattleEndCondition();
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
        //isProcessingAnswer = false;

        // 2. 아레나 모드: 나 혹은 상대방의 생명이 0인지 확인
        if (mode == BattleMode.Arena)
        {
            if (ExperienceSession.CurrentLife <= 0 || opponentLife <= 0)
            {
                Debug.Log("[아레나] 전투 종료 조건 충족");
                isProcessingAnswer = false;
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
