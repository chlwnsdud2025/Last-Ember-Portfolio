using UnityEngine;

public enum ItemType { Weapon, Armor, Artifact, Consumable, Material, KeyItem }
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

// abstract로 선언하여 이 클래스 자체는 생성하지 못하게 막습니다.
public abstract class ItemDataBase : ScriptableObject
{
    [Header("Core Data")]
    public string itemID; // JSON 저장/로드 및 DB 검색용 고유 ID (ex: "wpn_longsword_01")
    public string itemName;
    public ItemType itemType;
    public Rarity rarity;

    [Header("Visuals")]
    public Sprite icon;
    [TextArea(3, 5)] public string description;

    [Header("Settings")]
    public int maxStackAmount = 1; // 포션은 99, 무기는 1
    public int basePrice;
}