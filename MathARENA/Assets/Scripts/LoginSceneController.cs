using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginSceneController : MonoBehaviour
{
    // 임시 로그인 버튼에서 호출
    public void OnClickFakeLogin()
    {
        // TODO: 나중에 서버 로그인 성공 시에만 호출되게 연결
        SceneManager.LoadScene("02_Lobby");
    }
}
