using UnityEngine;

public class ProfileOverlayController : MonoBehaviour
{
    [Header("오버레이 패널들")]
    [SerializeField]
    private GameObject profileInfoRoot;

    [SerializeField]
    private GameObject inventoryOverlayRoot;

    [SerializeField]
    private GameObject rankingOverlayRoot;

    [Header("UI 시스템 참조")]
    [SerializeField]
    private InventoryUI inventoryUI;

    [SerializeField]
    private RankingUI rankingUI;

    private void Awake()
    {
        ShowProfile();
    }

    // [핵심] 모든 상태를 초기화하고 메인 프로필만 남기는 베이스 로직
    public void ShowProfile()
    {
        Debug.Log("[Profile] 모든 오버레이를 닫고 메인 프로필로 돌아갑니다.");

        if (profileInfoRoot != null)
            profileInfoRoot.SetActive(true);
        if (inventoryOverlayRoot != null)
            inventoryOverlayRoot.SetActive(false);
        if (rankingOverlayRoot != null)
            rankingOverlayRoot.SetActive(false);
    }

    public void OpenInventory()
    {
        // 1. 이미 인벤토리가 열려있다면? 토글해서 닫기
        if (inventoryOverlayRoot != null && inventoryOverlayRoot.activeSelf)
        {
            ShowProfile();
            return;
        }

        // 2. [강력한 상호 배제] 열기 전에 무조건 다 끄기
        CloseAllOverlays();

        // 3. 인벤토리만 활성화
        if (profileInfoRoot != null)
            profileInfoRoot.SetActive(false);
        if (inventoryOverlayRoot != null)
            inventoryOverlayRoot.SetActive(true);

        if (inventoryUI != null)
            inventoryUI.OpenDefaultTab();

        Debug.Log("[Profile] 인벤토리를 열었습니다. (랭킹 자동 종료)");
    }

    public void OpenRanking()
    {
        // 1. 이미 랭킹이 열려있다면? 토글해서 닫기
        if (rankingOverlayRoot != null && rankingOverlayRoot.activeSelf)
        {
            ShowProfile();
            return;
        }

        // 2. [강력한 상호 배제] 열기 전에 무조건 다 끄기
        CloseAllOverlays();

        // 3. 랭킹만 활성화
        if (profileInfoRoot != null)
            profileInfoRoot.SetActive(false);
        if (rankingOverlayRoot != null)
            rankingOverlayRoot.SetActive(true);

        if (rankingUI != null)
            rankingUI.OpenRanking();

        Debug.Log("[Profile] 랭킹을 열었습니다. (인벤토리 자동 종료)");
    }

    // 헬퍼 함수: 인벤토리와 랭킹을 둘 다 확실히 끔
    private void CloseAllOverlays()
    {
        if (inventoryOverlayRoot != null)
            inventoryOverlayRoot.SetActive(false);
        if (rankingOverlayRoot != null)
            rankingOverlayRoot.SetActive(false);
    }

    public void CloseOverlay()
    {
        ShowProfile();
    }
}
