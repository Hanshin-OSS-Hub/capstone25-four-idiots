using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리 화면 전체를 관리하는 UI 컨트롤러.
/// - 탭 버튼에서 OnClickChangeCategory(int) 를 호출해 카테고리 전환
/// - 현재 슬롯에 해당하는 아이템만 그리드에 카드로 생성
/// - 카드 프리팹에는 InventoryItemCardView 가 붙어 있어야 한다.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Grid Root (ScrollView Content)")]
    [SerializeField] private Transform itemGridRoot;      // ScrollView_Items/Viewport/Content
    [SerializeField] private GameObject itemCardPrefab;   // InventoryItemCard 프리팹

    [Header("Item Data (임시 / 나중엔 서버 데이터로 대체)")]
    [SerializeField] private List<InventoryItemData> debugItems = new List<InventoryItemData>();

    // 현재 선택된 슬롯(탭)
    private CostumeSlotType currentSlot = CostumeSlotType.Accessory;

    // 인벤토리 씬에 들어왔을 때는 아무 아이템도 보이지 않도록
    // Start 에서 자동 Refresh 는 호출하지 않는다.
    private void Start()
    {
        // 필요하다면 초기 탭을 강제로 띄우고 싶을 때만 사용:
        // Refresh(currentSlot);
    }

    /// <summary>
    /// 탭 버튼 OnClick(int) 에 연결되는 함수.
    /// 버튼 인스펙터에서 0~8 (enum 인덱스) 를 넘겨준다.
    /// </summary>
    public void OnClickChangeCategory(int slotInt)
    {
        currentSlot = (CostumeSlotType)slotInt;
        Refresh(currentSlot);
    }

    /// <summary>
    /// 현재 슬롯에 맞는 아이템만 다시 생성.
    /// </summary>
    public void Refresh(CostumeSlotType slot)
    {
        ClearGrid();

        // 해당 슬롯 아이템만 필터링해서 카드 생성
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

    /// <summary>
    /// 기존 카드 오브젝트 전부 삭제.
    /// </summary>
    private void ClearGrid()
    {
        // 뒤에서부터 Destroy 해야 안전
        for (int i = itemGridRoot.childCount - 1; i >= 0; i--)
        {
            var child = itemGridRoot.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}
