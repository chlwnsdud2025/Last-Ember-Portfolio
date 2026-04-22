using UnityEngine;

[CreateAssetMenu(fileName = "New Armor Data", menuName = "Inventory/Armor Data")]
public class ArmorData : ItemDataBase
{
    [Header("Armor Classification")]
    public ArmorType armorCategory; // 머리, 가슴, 다리, 발

    [Header("Armor Stats")]
    public float defense;           // 방어력
    public float weight;            // 무게 (이 수치에 따라 구르기 속도 결정)

    [Header("Visuals")]
    public GameObject armorModelPrefab; // (선택사항) 장착 시 캐릭터 외형 변경용 프리팹
}