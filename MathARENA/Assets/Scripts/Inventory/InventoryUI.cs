using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Grid Root (ScrollView Content)")]
    [SerializeField] private Transform itemGridRoot;
    [SerializeField] private GameObject itemCardPrefab;

    [Header("Item Data (임시 / 나중엔 서버 데이터로 대체)")]
    [SerializeField] private List<InventoryItemData> debugItems = new List<InventoryItemData>();

    [Header("Default")]
    [SerializeField] private CostumeSlotType defaultSlot = CostumeSlotType.Accessory;

    private CostumeSlotType currentSlot;
    private bool hasInitialized;

    private void Awake()
    {
        currentSlot = defaultSlot;
    }

    private void OnDisable()
    {
        ClearGrid();
    }

    public void OpenDefaultTab()
    {
        currentSlot = defaultSlot;
        Refresh(currentSlot);
        hasInitialized = true;
    }

    public void OnClickChangeCategory(int slotInt)
    {
        currentSlot = (CostumeSlotType)slotInt;
        Refresh(currentSlot);
        hasInitialized = true;
    }

    public void Refresh(CostumeSlotType slot)
    {
        if (itemGridRoot == null || itemCardPrefab == null)
        {
            Debug.LogWarning("[InventoryUI] itemGridRoot 또는 itemCardPrefab 참조가 비어 있습니다.");
            return;
        }

        ClearGrid();

        foreach (var item in debugItems)
        {
            if (item == null) continue;
            if (item.slot != slot) continue;

            var go = Instantiate(itemCardPrefab, itemGridRoot);
            var view = go.GetComponent<InventoryItemCardView>();
            if (view != null)
            {
                view.Setup(item);
            }
        }
    }

    private void ClearGrid()
    {
        if (itemGridRoot == null) return;

        for (int i = itemGridRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(itemGridRoot.GetChild(i).gameObject);
        }
    }

    public void SetItems(List<InventoryItemData> items)
    {
        debugItems = items ?? new List<InventoryItemData>();

        if (hasInitialized)
        {
            Refresh(currentSlot);
        }
    }
}