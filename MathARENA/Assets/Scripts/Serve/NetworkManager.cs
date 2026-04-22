using System;
using System.Collections;
using System.Collections.Generic;
using MathArena.Network;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField]
    private string baseUrl = "http://127.0.0.1:8001";
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

    // --- 로비에서 사용하는 프로필 요청 (복구됨) ---
    public void GetProfile(Action<UserProfileData> onSuccess, Action<string> onFail)
    {
        PostRequest<UserProfileData>("/v1/profile/me", new { }, onSuccess, onFail);
    }

    // --- 랭킹 리스트 요청 (복구됨) ---
    public void GetRankingList(
        Action<AuthResponse<List<RankingEntryData>>> onSuccess,
        Action<string> onFail
    )
    {
        PostRequest<AuthResponse<List<RankingEntryData>>>(
            "/v1/ranking",
            new { },
            onSuccess,
            onFail
        );
    }

    // --- 배틀 문제 요청 (5개 인자 버전) ---
    public void GetQuestion(
        string category,
        string difficulty,
        string excludeIds,
        Action<AuthResponse<ServerQuestionData>> onSuccess,
        Action<string> onFail
    )
    {
        var requestData = new
        {
            category = category,
            difficulty = difficulty,
            exclude_ids = excludeIds,
        };
        PostRequest<AuthResponse<ServerQuestionData>>(
            "/v1/battle/question",
            requestData,
            onSuccess,
            onFail
        );
    }

    // --- 배틀 결과 저장 ---
    public void SaveBattleResult(
        BattleResultRequest resultData,
        Action<AuthResponse<string>> onSuccess,
        Action<string> onFail
    )
    {
        PostRequest<AuthResponse<string>>("/v1/battle/save", resultData, onSuccess, onFail);
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
                T responseData = JsonUtility.FromJson<T>(request.downloadHandler.text);
                onSuccess?.Invoke(responseData);
            }
            else
            {
                onFail?.Invoke(request.downloadHandler.text);
            }
        }
    }
}
