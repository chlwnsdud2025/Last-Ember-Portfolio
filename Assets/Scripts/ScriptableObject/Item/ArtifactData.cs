using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact Data", menuName = "Inventory/Artifact Data")]
public class ArtifactData : ItemDataBase
{
    [Header("Artifact Effects")]
    public float attackPowerBoost;      // 공격력 증가량 (%)
    public float staminaRegenBoost;     // 스테미나 회복 가속 (%)
    public float maxHpBoost;            // 최대 체력 증가량

    [TextArea]
    public string loreText;             // 장신구에 담긴 특별한 배경 이야기
}