using UnityEngine;
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Data")]

[System.Serializable]
public class AttackData
{
    [Header("Animation")]
    public AnimationClip clip;

    [Header("Timings (0.0 ~ 1.0)")]
    [Tooltip("콤보 입력 가능 시작점")]
    public float comboWindowStart = 0.2f;
    [Tooltip("콤보 입력 가능 종료점")]
    public float comboWindowEnd = 0.7f;
    [Tooltip("다음 모션으로 실제 넘어가는 시점")]
    public float comboTransitionPoint = 0.6f;
    [Tooltip("이동/회피로 캔슬 가능한 시점")]
    public float cancelWindowStart = 0.7f;

    [Header("Stats")]
    [Tooltip("이 공격의 데미지 배율 (기본 데미지 * 배율)")]
    public float damageMultiplier = 1.0f;
}

// 2. 실제 무기 데이터 에셋 (이 클래스가 ScriptableObject를 상속받습니다)
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Data")]
public class Attack_SO_Data : ScriptableObject
{
    public string weaponName;

    [Header("Combo Attacks")]
    // 위에서 만든 AttackData 클래스를 배열로 가집니다.
    public AttackData[] comboAttacks;

    [Header("Stats")]
    public float baseDamage = 10f;
}