using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CommonButtonSFX : MonoBehaviour
{
    [SerializeField]
    private AudioClip clickSFX;

    private void Start()
    {
        // 버튼 클릭 이벤트에 효과음 재생 함수 연결
        GetComponent<Button>()
            .onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null && clickSFX != null)
                    AudioManager.Instance.PlaySFX(clickSFX);
            });
    }
}
