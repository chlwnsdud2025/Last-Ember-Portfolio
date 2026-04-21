using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable Data")]
public class ConsumableData : ItemDataBase
{
    [Header("Consumable Effects")]
    public float healAmount;
    public float buffDuration;
    public GameObject useEffectPrefab; // 마실 때 나오는 파티클
}