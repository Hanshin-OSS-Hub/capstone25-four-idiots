using UnityEngine;

// 코스튬/인벤토리 슬롯 종류 (탭과 1:1로 매칭)
public enum CostumeSlotType
{
    Illustration,   // 일러스트
    Character,      // 캐릭터
    Weapon,         // 무기
    Hair,           // 헤어
    Top,            // 상의
    Bottom,         // 하의
    Gloves,         // 장갑
    Shoes,          // 신발
    Accessory       // 악세서리
}

// 인벤토리 아이템 1개를 표현하는 데이터 구조
[System.Serializable]
public class InventoryItemData
{
    public string itemId;          // 내부 ID (서버/DB용)
    public string displayName;     // 표시 이름: "은 목걸이"

    [TextArea]
    public string description;     // 설명: "기본 목걸이"

    public Sprite icon;            // 아이콘 스프라이트
    public int count;              // 개수 (x1, x2 ...)

    public CostumeSlotType slot;   // 어떤 탭에 속하는지
}
