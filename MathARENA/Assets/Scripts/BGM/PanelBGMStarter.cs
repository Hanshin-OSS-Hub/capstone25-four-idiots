using UnityEngine;

public class PanelBGMStarter : MonoBehaviour
{
    [Header("이 패널이 켜질 때 재생할 브금")]
    [SerializeField]
    private AudioClip panelBGM;

    // 오브젝트가 SetActive(true) 될 때마다 호출됩니다.
    private void OnEnable()
    {
        if (AudioManager.Instance != null && panelBGM != null)
        {
            // AudioManager를 통해 평가창 브금으로 교체
            AudioManager.Instance.PlayBGM(panelBGM);
        }
    }
}
