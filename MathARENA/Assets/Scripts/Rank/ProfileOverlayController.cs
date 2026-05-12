using MathArena.Network; // [추가] 네트워크 데이터 타입을 사용하기 위해 필요합니다.
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
    private ProfileUIController profileUI; // [핵심 추가] 프로필 UI를 갱신할 컨트롤러 연결

    [SerializeField]
    private InventoryUI inventoryUI;

    [SerializeField]
    private RankingUI rankingUI;

    private void Awake()
    {
        ShowProfile();
    }

    // [수정] 화면을 보여줄 때마다 서버 정보를 새로고침합니다.
    public void ShowProfile()
    {
        Debug.Log("[Profile] 메인 프로필로 돌아오며 데이터를 새로고침합니다.");

        if (profileInfoRoot != null)
            profileInfoRoot.SetActive(true);
        if (inventoryOverlayRoot != null)
            inventoryOverlayRoot.SetActive(false);
        if (rankingOverlayRoot != null)
            rankingOverlayRoot.SetActive(false);

        // [핵심 로직 추가] 서버에서 최신 프로필 정보를 가져와 UI에 적용
        RefreshProfileFromServer();
    }

    private void RefreshProfileFromServer()
    {
        if (NetworkManager.Instance == null || profileUI == null)
            return;

        NetworkManager.Instance.GetProfile(
            (data) =>
            {
                // [핵심 추가] 불러온 데이터를 세션에 저장해야 배틀 컨트롤러가 내 현재 BP를 압니다!
                ExperienceSession.UserProfile = data;

                profileUI.UpdateProfileUI(data);
                Debug.Log($"[Profile] {data.nickname}님의 데이터를 세션에 저장했습니다.");
            },
            (err) => Debug.LogError($"[Profile] 로드 실패: {err}")
        );
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
