using UnityEngine;

public class ProfileInventoryOverlayController : MonoBehaviour
{
    [SerializeField] private GameObject profileInfoRoot;
    [SerializeField] private GameObject inventoryOverlayRoot;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private int defaultOpenTab = 8;

    private void Awake()
    {
        if (profileInfoRoot != null)
            profileInfoRoot.SetActive(true);

        if (inventoryOverlayRoot != null)
            inventoryOverlayRoot.SetActive(false);
    }

    public void OpenInventory()
    {
        if (profileInfoRoot != null)
            profileInfoRoot.SetActive(false);

        if (inventoryOverlayRoot != null)
            inventoryOverlayRoot.SetActive(true);

        if (inventoryUI != null)
            inventoryUI.OnClickChangeCategory(defaultOpenTab);
    }

    public void CloseInventory()
    {
        if (inventoryOverlayRoot != null)
            inventoryOverlayRoot.SetActive(false);

        if (profileInfoRoot != null)
            profileInfoRoot.SetActive(true);
    }
}