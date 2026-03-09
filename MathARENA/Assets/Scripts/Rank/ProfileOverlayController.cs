using UnityEngine;

public class ProfileOverlayController : MonoBehaviour
{
    [SerializeField] private GameObject profileInfoRoot;
    [SerializeField] private GameObject inventoryOverlayRoot;
    [SerializeField] private GameObject rankingOverlayRoot;

    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private RankingUI rankingUI;

    private void Awake()
    {
        ShowProfile();
    }

    public void ShowProfile()
    {
        if (profileInfoRoot != null) profileInfoRoot.SetActive(true);
        if (inventoryOverlayRoot != null) inventoryOverlayRoot.SetActive(false);
        if (rankingOverlayRoot != null) rankingOverlayRoot.SetActive(false);
    }

    public void OpenInventory()
    {
        if (profileInfoRoot != null) profileInfoRoot.SetActive(false);
        if (inventoryOverlayRoot != null) inventoryOverlayRoot.SetActive(true);
        if (rankingOverlayRoot != null) rankingOverlayRoot.SetActive(false);

        if (inventoryUI != null)
            inventoryUI.OpenDefaultTab();
    }

    public void OpenRanking()
    {
        if (profileInfoRoot != null) profileInfoRoot.SetActive(false);
        if (inventoryOverlayRoot != null) inventoryOverlayRoot.SetActive(false);
        if (rankingOverlayRoot != null) rankingOverlayRoot.SetActive(true);

        if (rankingUI != null)
            rankingUI.OpenRanking();
    }

    public void CloseOverlay()
    {
        ShowProfile();
    }
}