using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneController : MonoBehaviour
{
    public void OnClickBackToLogin()
    {
        SceneManager.LoadScene("01_Login");
    }
}
