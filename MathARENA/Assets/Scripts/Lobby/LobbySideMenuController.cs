using UnityEngine;

public class LobbySideMenuController : MonoBehaviour
{
    [SerializeField] private GameObject sideMenuRoot;  // Panel_SideMenuRoot

    private void Awake()
    {
        if (sideMenuRoot != null)
        {
            // 시작할 때는 무조건 사이드메뉴를 꺼 둔다
            sideMenuRoot.SetActive(false);
        }
    }

    public void OpenMenu()
    {
        Debug.Log("[LobbySideMenu] OpenMenu");   // 버튼 눌렸는지 확인용 로그

        if (sideMenuRoot != null)
        {
            sideMenuRoot.SetActive(true);
        }
    }

    public void CloseMenu()
    {
        Debug.Log("[LobbySideMenu] CloseMenu");

        if (sideMenuRoot != null)
        {
            sideMenuRoot.SetActive(false);
        }
    }

    public void ToggleMenu()
    {
        Debug.Log("[LobbySideMenu] ToggleMenu");

        if (sideMenuRoot != null)
        {
            sideMenuRoot.SetActive(!sideMenuRoot.activeSelf);
        }
    }
}
