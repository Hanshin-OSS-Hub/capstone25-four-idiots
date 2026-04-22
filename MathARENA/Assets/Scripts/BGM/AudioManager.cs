using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource bgmSource; // BGM용

    [SerializeField]
    private AudioSource sfxSource; // SFX용 (이게 있어야 에러가 안 납니다)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // BGM 재생 로직
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip)
            return;
        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // [에러 해결 포인트] 효과음 재생 함수
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
