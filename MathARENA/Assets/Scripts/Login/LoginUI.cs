using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField]
    private TMP_InputField idInput;

    [SerializeField]
    private TMP_InputField pwInput;

    [Header("Panels")]
    [SerializeField]
    private GameObject loginPanel; // Panel_Card 등 (지금은 유지)

    [SerializeField]
    private GameObject popupRoot; // ★ Panel_PopupRoot (회원가입 팝업 루트)

    // ***** 로그인 버튼 *****
    public void OnClickLogin()
    {
        string id = idInput.text;
        string pw = pwInput.text;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            Debug.LogWarning("아이디/비밀번호를 입력하세요.");
            return;
        }

        Debug.Log($"[Login Try] id={id}, pw={pw}");

        // 임시: 로그인 성공 가정 후 로비로 이동
        SceneManager.LoadScene("02_Lobby");
    }

    // ***** 구글 로그인 버튼 *****
    public void OnClickGoogleLogin()
    {
        Debug.Log("[Login] Google Login Clicked");
    }

    // ***** 애플 로그인 버튼 *****
    public void OnClickAppleLogin()
    {
        Debug.Log("[Login] Apple Login Clicked");
    }

    // ***** 회원가입 버튼 (팝업 열기) *****
    public void OnClickSignUp()
    {
        if (popupRoot == null)
        {
            Debug.LogWarning("[Login] popupRoot(Panel_PopupRoot) is not set.");
            return;
        }

        popupRoot.SetActive(true);
    }

    // ***** 계정 찾기 버튼 *****
    public void OnClickFindAccount()
    {
        Debug.Log("[Login] Find Account Clicked");
    }
}
