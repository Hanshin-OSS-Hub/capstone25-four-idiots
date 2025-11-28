using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemCardView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;

    [Header("Count Texts (Separated)")]
    [SerializeField] private TMP_Text countXText;      // "x"
    [SerializeField] private TMP_Text countNumberText; // "1"

    [Header("Button")]
    [SerializeField] private Button equipButton;

    private InventoryItemData itemData;

    public void Setup(InventoryItemData data)
    {
        itemData = data;

        // ----- 아이콘 -----
        if (iconImage != null)
            iconImage.sprite = data.icon;

        // ----- 이름 -----
        if (nameText != null)
            nameText.text = data.displayName;

        // ----- 설명 -----
        if (descText != null)
            descText.text = data.description;

        // ----- 개수 표시 (분리된 방식) -----
        if (countXText != null)
            countXText.text = "x";

        if (countNumberText != null)
            countNumberText.text = data.count.ToString();

        // ----- 버튼 콜백 -----
        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(OnClickEquip);
        }
    }

    private void OnClickEquip()
    {
        Debug.Log($"[Inventory] Equip item: {itemData?.itemId}");
        // TODO: 실제 장착 로직 연결
    }
}
