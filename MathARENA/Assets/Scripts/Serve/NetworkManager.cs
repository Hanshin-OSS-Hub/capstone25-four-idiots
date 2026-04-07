using System;
using System.Collections;
using MathArena.Network;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [Header("Server Config")]
    [SerializeField]
    private string baseUrl = "http://127.0.0.1:8001"; //

    // 서버에서 받은 JWT 토큰 저장용
    public string AccessToken { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지
        }
        else
        {
            Destroy(gameObject);
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

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 토큰이 있다면 헤더에 추가 (보안)
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

    public void SetToken(string token) => AccessToken = token;
}
