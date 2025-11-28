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
    private GameObject loginPanel; // Panel_Card 등

    [SerializeField]
    private GameObject signUpPanel; // 나중에 쓸 회원가입 패널 (처음엔 null 가능)

    // ***** 로그인 버튼 *****
    public void OnClickLogin()
    {
        string id = idInput.text;
        string pw = pwInput.text;

        // TODO: 간단한 빈값 체크 정도는 여기서 가능
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            Debug.LogWarning("아이디/비밀번호를 입력하세요.");
            return;
        }

        // TODO: 여기서 서버 로그인 API 호출 (백엔드 팀이 채울 부분)
        Debug.Log($"[Login Try] id={id}, pw={pw}");

        // 임시: 로그인 성공 가정 후 로비로 이동
        SceneManager.LoadScene("02_Lobby");
    }

    // ***** 구글 로그인 버튼 *****
    public void OnClickGoogleLogin()
    {
        // TODO: 추후 Android/iOS Google SDK 연동 위치
        Debug.Log("[Login] Google Login Clicked");
    }

    // ***** 애플 로그인 버튼 *****
    public void OnClickAppleLogin()
    {
        // TODO: 추후 Apple 로그인 연동 위치
        Debug.Log("[Login] Apple Login Clicked");
    }

    // ***** 회원가입 버튼 (패널 전환 용) *****
    public void OnClickSignUp()
    {
        if (signUpPanel == null || loginPanel == null)
        {
            Debug.Log("[Login] SignUp Panel not set yet.");
            return;
        }

        loginPanel.SetActive(false);
        signUpPanel.SetActive(true);
    }

    // ***** 계정 찾기 버튼 *****
    public void OnClickFindAccount()
    {
        // 나중에 팝업/웹뷰 등으로 확장
        Debug.Log("[Login] Find Account Clicked");
    }
}
