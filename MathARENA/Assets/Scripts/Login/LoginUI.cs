using MathArena.Network;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField idInput;

    [SerializeField]
    private TMP_InputField pwInput;

    [SerializeField]
    private GameObject popupRoot;

    public void OnClickLogin()
    {
        if (string.IsNullOrEmpty(idInput.text) || string.IsNullOrEmpty(pwInput.text))
            return;

        LoginRequest data = new LoginRequest { id = idInput.text, pw = pwInput.text };

        NetworkManager.Instance.PostRequest<AuthResponse<LoginData>>(
            "/v1/auth/login",
            data,
            (res) =>
            {
                if (res.success)
                {
                    NetworkManager.Instance.SetToken(res.data.access_token);
                    Debug.Log($"Welcome, {res.data.nickname}!");
                    SceneManager.LoadScene("02_Lobby"); // 로비로 이동
                }
                else
                {
                    Debug.LogError("Error: " + res.error.message);
                }
            },
            (err) => Debug.LogError($"[Network Error Detail] {err}")
        );
    }

    public void OnClickSignUp() => popupRoot?.SetActive(true); // 회원가입 팝업
}
