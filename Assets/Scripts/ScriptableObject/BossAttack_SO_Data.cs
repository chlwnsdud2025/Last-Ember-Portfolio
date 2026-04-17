using UnityEngine;
[CreateAssetMenu(fileName = "New Boss Attack", menuName = "Boss/Attack Data")]
public class BossAttack_SO_Data : ScriptableObject
{
    [Header("액션 및 타이밍 (AttackState용)")]
    //  핵심: string 이름 대신 AnimationClip을 직접 받습니다!
    public AnimationClip attackClip;

    [Header("발동 조건 (Decision용)")]
    public string attackName;
    public float minDistance = 0f;
    public float maxDistance = 4f;
    [Range(-180f, 180f)] public float minAngle = -60f; // 보스 정면 기준 유효 각도
    [Range(-180f, 180f)] public float maxAngle = 60f;
    public float cooldown = 5f;
    public int weight = 10;

    public int requiredPhase = 1; // 특정 페이즈 전용 여부

    [HideInInspector] public float lastUsedTime = -999f;

    public bool canTrackTarget = true;  // 공격 중 플레이어를 계속 바라볼지


    [Header("액션 및 타이밍 (AttackState용)")]
    public string animationName;

    [Range(0f, 1f)] public float hitboxStartTime = 0.3f; // 히트박스 활성화 시점
    [Range(0f, 1f)] public float hitboxEndTime = 0.5f;   // 히트박스 비활성화 시점
    [Range(0f, 1f)] public float comboEnableTime = 0.6f; // 다음 콤보 입력 가능/연계 시점
    [Range(0f, 1f)] public float attackEndTime = 0.9f;   // 공격 전체 종료 및 Decision 복귀 시점

    [Header("공격 스탯")]
    public float damage = 20f;
    public bool trackTarget = true;

    [Header("연계 패턴")]
    public BossAttack_SO_Data nextCombo;
}
