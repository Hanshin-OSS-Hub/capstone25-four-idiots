using UnityEngine;

public class LobbyBGMStarter : MonoBehaviour
{
    [SerializeField] private AudioClip lobbyBGM;

    private void Start()
    {
        AudioManager.Instance.PlayBGM(lobbyBGM);
    }
}