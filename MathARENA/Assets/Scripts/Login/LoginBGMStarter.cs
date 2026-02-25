using UnityEngine;

public class LoginBGMStarter : MonoBehaviour
{
    [SerializeField] private AudioClip loginBGM;

    private void Start()
    {
        AudioManager.Instance.PlayBGM(loginBGM);
    }
}