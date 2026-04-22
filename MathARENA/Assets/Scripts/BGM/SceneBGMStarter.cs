using UnityEngine;

public class SceneBGMStarter : MonoBehaviour
{
    [Header("이 씬에서 재생할 브금")]
    [SerializeField] private AudioClip sceneBGM;

    private void Start()
    {
        // AudioManager가 존재하고 브금이 설정되어 있을 때만 재생
        if (AudioManager.Instance != null && sceneBGM != null)
        {
            AudioManager.Instance.PlayBGM(sceneBGM);
        }
    }
}