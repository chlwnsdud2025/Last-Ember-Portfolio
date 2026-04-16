using UnityEngine;

[System.Serializable]
public struct HitTiming
{
    public float startTime;
    public float endTime;
}

[CreateAssetMenu(fileName = "New Enemy Attack", menuName = "Enemy/Attack Data")]
public class EnemyAttack_SO_Data : ScriptableObject
{
    [Header("Animation Info")]
    public string attackAnimName;
    public AnimationClip clip;

    [Header("AI Conditions (발동 조건)")]
    public float minAttackRange = 0f;
    public float maxAttackRange = 3f;
    public float cooldown = 5f;
    public int priorityWeight = 10;

    [Header("Timings (0.0 ~ 1.0)")]
    public float trackingWindowEnd = 0.2f;
    public float transitionPoint = 0.9f;

    // ---------------------------------------------------
    //  [핵심 2] 다중 타격 및 무기 선택 시스템 적용!
    // ---------------------------------------------------
    [Header("Hitbox Selection & Multi-Hit")]
    [Tooltip("0번: 칼, 1번: 방패 등 에너미 컨트롤러에 등록된 무기 인덱스")]
    public int weaponIndex = 0;

    [Tooltip("판정이 켜졌다 꺼지는 여러 구간들 (예: 1타 0.2~0.3, 2타 0.6~0.7)")]
    public HitTiming[] hitTimings;
    // ---------------------------------------------------

    [Header("Stats")]
    public float damageMultiplier = 1.0f;
    public float poiseDamage = 10f;
}
