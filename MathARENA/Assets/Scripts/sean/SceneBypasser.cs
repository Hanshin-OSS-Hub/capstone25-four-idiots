using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBypasser : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "02_Lobby"; // 넘어갈 씬 이름

    public void OnClickSkipLogin()
    {
        Debug.Log("[Debug] 서버 연동 없이 로비로 진입합니다.");
        SceneManager.LoadScene(nextSceneName);
    }
}