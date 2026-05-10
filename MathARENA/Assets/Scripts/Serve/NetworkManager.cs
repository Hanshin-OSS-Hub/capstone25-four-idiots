using System;
using System.Collections;
using System.Collections.Generic;
using MathArena.Network;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [Header("Server Settings")]
    [SerializeField]
    private string baseUrl = "https://capstone25-four-idiots-1.onrender.com"; // [수정] 로그에 찍힌 실서버 주소 반영

    public string BaseUrl => baseUrl;
    public string AccessToken { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetToken(string token)
    {
        AccessToken = token;
    }

    // [1] 프로필 (GET)
    public void GetProfile(Action<UserProfileData> onSuccess, Action<string> onFail)
    {
        GetRequest<UserProfileData>("/v1/user/profile", onSuccess, onFail);
    }

    // [2] 체험장 (POST / GET)
    // [수정] Start API는 이제 ExperienceStartData를 반환합니다.
    public void StartExperience(
        string cat,
        string diff,
        string ids,
        Action<AuthResponse<ExperienceStartData>> onSuccess,
        Action<string> onFail
    )
    {
        QuestionRequest data = new QuestionRequest
        {
            category = cat,
            difficulty = diff,
            exclude_ids = ids,
        };
        PostRequest("/v1/experience/start", data, onSuccess, onFail);
    }

    // [추가] 세션 ID로 실제 문제 데이터를 가져오는 전용 함수
    public void GetExperienceQuestion(
        string sessionId,
        Action<AuthResponse<ServerQuestionData>> onSuccess,
        Action<string> onFail
    )
    {
        // 쿼리 스트링(?session_id=...) 방식으로 전달합니다.
        string endpoint = $"/v1/experience/question?session_id={sessionId}";
        GetRequest<AuthResponse<ServerQuestionData>>(endpoint, onSuccess, onFail);
    }

    // NetworkManager.cs 내부
    public void SubmitExperience(
        BattleResultRequest data,
        Action<AuthResponse<ExperienceSubmitResponse>> onSuccess, // [수정] ExperienceSubmitResponse 사용
        Action<string> onFail
    )
    {
        PostRequest("/v1/experience/submit", data, onSuccess, onFail);
    }

    // [3] 훈련장 (POST)
    // [참고] 훈련장도 동일한 구조라면 나중에 GetTrainingQuestion 등을 추가해야 할 수 있습니다.
    public void StartTraining(
        string cat,
        string diff,
        string ids,
        Action<AuthResponse<ServerQuestionData>> onSuccess,
        Action<string> onFail
    )
    {
        QuestionRequest data = new QuestionRequest
        {
            category = cat,
            difficulty = diff,
            exclude_ids = ids,
        };
        PostRequest("/v1/training/start", data, onSuccess, onFail);
    }

    public void SubmitTraining(
        BattleResultRequest data,
        Action<AuthResponse<string>> onSuccess,
        Action<string> onFail
    )
    {
        PostRequest("/v1/training/submit", data, onSuccess, onFail);
    }

    // [4] 아레나 (GET/POST)
    public void GetMatchRecommendations(
        Action<AuthResponse<List<RankingEntryData>>> onSuccess,
        Action<string> onFail
    )
    {
        GetRequest("/v1/match/recommendations", onSuccess, onFail);
    }

    public void StartMatch(
        string cat,
        string diff,
        Action<AuthResponse<ServerQuestionData>> onSuccess,
        Action<string> onFail
    )
    {
        MatchRequest data = new MatchRequest { category = cat, difficulty = diff };
        PostRequest("/v1/match/start", data, onSuccess, onFail);
    }

    public void SubmitMatch(
        BattleResultRequest data,
        Action<AuthResponse<string>> onSuccess,
        Action<string> onFail
    )
    {
        PostRequest("/v1/match/submit", data, onSuccess, onFail);
    }

    // [5] 랭킹 (POST)
    public void GetRankingList(
        Action<AuthResponse<List<RankingEntryData>>> onSuccess,
        Action<string> onFail
    )
    {
        PostRequest("/v1/ranking", new EmptyRequest(), onSuccess, onFail);
    }

    // --- 공통 통신 메서드 ---
    public void GetRequest<T>(string endpoint, Action<T> onSuccess, Action<string> onFail)
    {
        StartCoroutine(GetCoroutine(endpoint, onSuccess, onFail));
    }

    private IEnumerator GetCoroutine<T>(string endpoint, Action<T> onSuccess, Action<string> onFail)
    {
        string fullUrl = baseUrl + endpoint;
        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            if (!string.IsNullOrEmpty(AccessToken))
                request.SetRequestHeader("Authorization", "Bearer " + AccessToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(
                    $"[GET Response Success] URL: {endpoint}\nRaw: {request.downloadHandler.text}"
                );
                onSuccess?.Invoke(JsonUtility.FromJson<T>(request.downloadHandler.text));
            }
            else
            {
                Debug.LogError(
                    $"[GET Response Error] URL: {endpoint}\nError: {request.downloadHandler.text}"
                );
                onFail?.Invoke(request.downloadHandler.text);
            }
        }
    }

    public void PostRequest<T>(
        string endpoint,
        object postData,
        Action<T> onSuccess,
        Action<string> onFail
    )
    {
        StartCoroutine(PostCoroutine(endpoint, postData, onSuccess, onFail));
    }

    private IEnumerator PostCoroutine<T>(
        string endpoint,
        object postData,
        Action<T> onSuccess,
        Action<string> onFail
    )
    {
        string fullUrl = baseUrl + endpoint;
        string json = JsonUtility.ToJson(postData);

        Debug.Log($"[POST Request] URL: {fullUrl}\nJSON: {json}");

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(AccessToken))
                request.SetRequestHeader("Authorization", "Bearer " + AccessToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(
                    $"[POST Response Success] URL: {endpoint}\nRaw: {request.downloadHandler.text}"
                );
                onSuccess?.Invoke(JsonUtility.FromJson<T>(request.downloadHandler.text));
            }
            else
            {
                Debug.LogError(
                    $"[POST Response Error] URL: {endpoint}\nError: {request.downloadHandler.text}"
                );
                onFail?.Invoke(request.downloadHandler.text);
            }
        }
    }
}
