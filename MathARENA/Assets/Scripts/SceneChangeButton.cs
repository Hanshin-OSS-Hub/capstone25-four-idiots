using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeButton : MonoBehaviour
{
    [SerializeField]
    private string targetSceneName;

    // UI 버튼 OnClick 에 연결해서 사용
    public void OnClickChangeScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"{name} : targetSceneName 이 비어있습니다.");
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }
}
