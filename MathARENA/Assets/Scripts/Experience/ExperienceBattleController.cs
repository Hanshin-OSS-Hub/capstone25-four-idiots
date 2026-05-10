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

    //private string currentDiffName = "VERY EASY";
    private int opponentLife = 4;
    private int currentOpponentRecordIndex = 0;

    private ExperienceCategory currentCategory;
    private string correctAnswerText = "";
    private int totalQuestionLimit = 30;

    private bool isProcessingAnswer = false; // 155번 줄 (중복 제거됨)

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
        {
            opponentLife = 4;

            // 만약 상대 닉네임을 표시할 TMP_Text가 있다면 여기서 연결
            // if (opponentNameText != null) opponentNameText.text = ArenaSession.OpponentId;

            UpdateLifeUI();
            LoadArenaOpponentData();
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
        // [수정] 서버 통신 로직을 모두 지우고 즉시 문제 세팅으로 넘어갑니다.
        Debug.Log($"[아레나] 로컬 샘플 데이터로 매칭 완료: {ArenaSession.OpponentId}");

        // 상대방 전적(샘플)이 필요하다면 여기서 리스트에 더미 데이터를 넣을 수도 있습니다.
        opponentRecords.Clear();

        // 다음 단계(문제 불러오기)로 즉시 이동
        SetupQuestionByCategory();
    }

    // ExperienceBattleController.cs의 SetupQuestionByCategory 메서드 수정
    private void SetupQuestionByCategory()
    {
        if (loadingOverlay != null)
            loadingOverlay.SetActive(true); // 로딩 켜기

        // [핵심 수정] 현재 카테고리를 string으로 변환 후 소문자(ToLower)로 만들어 서버 규격에 맞춥니다.
        string cat = currentCategory.ToString().ToLower();
        string diff = DetermineDifficulty(); // 기존 currentDiffName 대신 동적 난이도 결정 함수 사용 권장

        Debug.Log($"[서버 요청 시작] 카테고리: {cat}, 난이도: {diff}");

        // 1단계: 세션 생성 (Start)
        NetworkManager.Instance.StartExperience(
            cat,
            diff,
            "",
            (startRes) =>
            {
                if (startRes.success && startRes.data != null)
                {
                    string sessionId = startRes.data.session_id;
                    Debug.Log($"[세션 생성 성공] Session ID: {sessionId}");

                    // 2단계: 받은 session_id로 실제 문제 요청 (Question)
                    NetworkManager.Instance.GetExperienceQuestion(
                        sessionId,
                        (questionRes) =>
                        {
                            if (questionRes.success && questionRes.data != null)
                            {
                                // 실제 문제 데이터(content, choices 등)를 UI에 적용
                                ApplyServerDataToUI(questionRes.data);

                                if (loadingOverlay != null)
                                    loadingOverlay.SetActive(false); // 로딩 끄기
                            }
                        },
                        (err) =>
                        {
                            Debug.LogError($"문제 로드 실패: {err}");
                            isProcessingAnswer = false; // 실패 시 다시 시도할 수 있게 잠금 해제
                            if (loadingOverlay != null)
                                loadingOverlay.SetActive(false);
                        }
                    );
                }
                else
                {
                    Debug.LogWarning("[세션 생성 실패] 응답이 성공이 아니거나 데이터가 없습니다.");
                    if (loadingOverlay != null)
                        loadingOverlay.SetActive(false);
                }
            },
            (err) =>
            {
                Debug.LogError($"세션 생성 통신 에러: {err}");
                if (loadingOverlay != null)
                    loadingOverlay.SetActive(false);
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

    // ExperienceBattleController.cs의 ApplyServerDataToUI 함수 전문 (334번 줄 근처)
    private void ApplyServerDataToUI(ServerQuestionData data)
    {
        if (this == null || !gameObject.activeInHierarchy || data == null)
            return;

        // 1. 상태 초기화
        isProcessingAnswer = false;
        questionStartTime = Time.time;
        currentSessionId = data.session_id;
        currentQuestionId = !string.IsNullOrEmpty(data.question_id) ? data.question_id : data.q_id;

        // [핵심 수정] 훈련장 진입 시 선택한 카테고리를 강제로 다시 가져옵니다.
        currentCategory = ExperienceSession.CurrentCategory;

        // 2. 현재 카테고리에 따른 패널 스위칭
        bool isDesignMode = (currentCategory == ExperienceCategory.Design);
        bool isOCRMode = (
            currentCategory == ExperienceCategory.Calc
            || currentCategory == ExperienceCategory.Practice
        );

        if (panelChoices != null)
            panelChoices.SetActive(!isDesignMode && !isOCRMode);
        if (panelDesign != null)
            panelDesign.SetActive(isDesignMode);
        if (panelInput != null)
            panelInput.SetActive(isOCRMode);

        Debug.Log($"[UI 적용] 현재 모드: {currentCategory}, 패널 설정 완료");

        // 3. 정답 데이터 파싱
        if (isDesignMode)
        {
            if (!string.IsNullOrEmpty(data.correct_answer))
            {
                correctAnswerText = data.correct_answer;
                string[] parts = correctAnswerText.Split('-');
                List<int> parsedSequence = new List<int>();
                foreach (string p in parts)
                {
                    if (int.TryParse(p, out int val))
                        parsedSequence.Add(val - 1); // 인덱스 보정
                }
                designCorrectSequence = parsedSequence.ToArray();
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(data.correct_answer))
                correctAnswerText = data.correct_answer;
            else if (data.answer_val != 0)
                correctAnswerText = data.answer_val.ToString();
        }

        // 4. 지문 설정
        string contentToShow = !string.IsNullOrEmpty(data.content) ? data.content : data.text;
        if (questionText != null && !string.IsNullOrEmpty(contentToShow))
        {
            questionText.text = contentToShow.Replace("$", "");
        }

        // 5. 모드별 상세 UI 초기화
        if (isOCRMode)
        {
            if (inputField != null)
                inputField.text = "입력된 숫자 : ";
            drawingCanvas?.ClearCanvas();
        }
        else if (isDesignMode)
        {
            designUserSequence.Clear();
            designOriginalTexts = new string[data.choices.Count];
            for (int i = 0; i < designChoiceTexts.Length; i++)
            {
                if (i < data.choices.Count)
                {
                    designOriginalTexts[i] = data.choices[i].Replace("$", "");
                    designChoiceTexts[i].text = designOriginalTexts[i];
                    designChoiceButtons[i].gameObject.SetActive(true);
                    // 투명도 초기화
                    Color c = designChoiceButtons[i].image.color;
                    c.a = 1.0f;
                    designChoiceButtons[i].image.color = c;
                }
                else
                    designChoiceButtons[i].gameObject.SetActive(false);
            }
        }
        else // 객관식
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

        // 6. 버튼 연결 및 카운터 갱신
        HookAllButtons();
        UpdateQuestionCounterUI();
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
            if (opponentRecords != null && currentOpponentRecordIndex < opponentRecords.Count)
            {
                bool isOpponentCorrect = opponentRecords[currentOpponentRecordIndex].is_correct;
                Debug.Log($"상대방 {currentOpponentRecordIndex}번 문제 결과: {isOpponentCorrect}");

                // 사용했으므로 인덱스 증가
                currentOpponentRecordIndex++;
            }
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
        BattleResultRequest request = new BattleResultRequest
        {
            category_name = currentCategory.ToString(),
            total_score = score,
            results = battleRecords,
        };
        // 기존 SaveBattleResult를 SubmitExperience로 변경합니다.
        NetworkManager.Instance.SubmitExperience(
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
    private void ExecuteResultSequence(bool isCorrect, string answerForServer)
    {
        isProcessingAnswer = true;

        // 1. 하트 차감 로직 (아레나 및 훈련장 대응)
        if (mode == BattleMode.Arena)
        {
            if (isCorrect)
            {
                // [승리 조건] 내가 맞히면 상대방의 하트를 하나 깎습니다.
                opponentLife--;
                if (opponentLife < 0)
                    opponentLife = 0;
                Debug.Log($"[아레나] 정답! 상대방 체력 차감. 남은 체력: {opponentLife}");
            }
            else
            {
                // [패배 조건] 내가 틀리면 나의 하트를 하나 깎습니다.
                ExperienceSession.CurrentLife--;
                if (ExperienceSession.CurrentLife < 0)
                    ExperienceSession.CurrentLife = 0;
                Debug.Log(
                    $"[아레나] 오답... 내 체력 차감. 남은 체력: {ExperienceSession.CurrentLife}"
                );
            }
            UpdateLifeUI(); // UI에 반영 (내 하트와 상대 하트 모두 갱신)
        }
        else if (mode == BattleMode.Training)
        {
            // 훈련장 모드에서는 오답일 때만 내 하트를 깎습니다.
            if (!isCorrect)
            {
                ExperienceSession.CurrentLife--;
                if (ExperienceSession.CurrentLife < 0)
                    ExperienceSession.CurrentLife = 0;
                Debug.Log($"[훈련장] 오답. 남은 하트: {ExperienceSession.CurrentLife}");
                UpdateLifeUI();
            }
        }

        // 2. 판정 연출 및 씬 관리
        if (sequenceManager != null && sequenceManager.gameObject.activeInHierarchy)
        {
            sequenceManager.OnSequenceComplete = () =>
            {
                // 다음 문제를 위해 설계 모드 선택 데이터 초기화
                designUserSequence.Clear();
                isProcessingAnswer = false;

                // 3. 종료 조건 검사
                if (mode == BattleMode.Arena)
                {
                    // 아레나: 누군가의 하트가 0이 되면 즉시 종료
                    if (opponentLife <= 0 || ExperienceSession.CurrentLife <= 0)
                    {
                        FinishBattle();
                    }
                    else
                    {
                        ContinueBattleProcess();
                    }
                }
                else if (mode == BattleMode.Training)
                {
                    // 훈련장: 내 하트가 0이 되면 즉시 종료
                    if (ExperienceSession.CurrentLife <= 0)
                    {
                        FinishBattle();
                    }
                    else
                    {
                        ContinueBattleProcess();
                    }
                }
                else
                {
                    // 경험 모드 등은 하트 상관없이 계속 진행
                    ContinueBattleProcess();
                }
            };
            sequenceManager.PlaySequence(isCorrect);
        }
        else
        {
            // 연출 매니저가 없는 경우에도 동일한 종료 조건 로직 수행
            designUserSequence.Clear();
            isProcessingAnswer = false;

            if (mode == BattleMode.Arena)
            {
                if (opponentLife <= 0 || ExperienceSession.CurrentLife <= 0)
                    FinishBattle();
                else
                    ContinueBattleProcess();
            }
            else if (mode == BattleMode.Training && ExperienceSession.CurrentLife <= 0)
            {
                FinishBattle();
            }
            else
            {
                ContinueBattleProcess();
            }
        }

        // 서버에 기록 전송 (답변 및 정답 여부)
        SendRecordToServer(answerForServer, isCorrect);
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
}
