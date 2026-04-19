using UnityEngine;

public enum RotationTrackingType
{
    None,               // 아예 안 쳐다봄 (광역기 등)
    OnlyStartup,        // 선딜레이 구간에만 쳐다봄 (가장 일반적인 소울류 공격)
    Continuous          // 공격 내내 쳐다봄 (레이저 빔, 휠윈드 등)
}

[CreateAssetMenu(fileName = "New Boss Attack", menuName = "Boss/AAA Attack Data")]
public class BossAttack_SO_Data : ScriptableObject
{
    [Header(" 1. 발동 조건 (Decision 체계)")]
    [Tooltip("디버그용 공격 이름")]
    public string attackName = "기본 베기";

    [Tooltip("이 공격이 발동되기 위한 최소/최대 거리")]
    public float minDistance = 0f;
    public float maxDistance = 5f;

    [Tooltip("보스 정면을 기준으로 플레이어가 이 각도 안에 있어야 발동 (-180 ~ 180)")]
    [Range(-180f, 180f)] public float validMinAngle = -45f;
    [Range(-180f, 180f)] public float validMaxAngle = 45f;

    [Tooltip("사용 후 다시 뽑히기까지의 대기 시간")]
    public float cooldown = 3f;
    public int weight = 50;

    [Tooltip("이 공격이 해금되는 보스의 페이즈 (1페이즈엔 안 쓰는 패턴 등)")]
    public int requiredPhase = 1;

    [Space(10)]
    [Header(" 2. 애니메이션 제어 (Animation Control)")]
    public AnimationClip attackClip;

    [Tooltip("애니메이션 전환(CrossFade)에 걸리는 부드러운 시간 (보통 0.1 ~ 0.2)")]
    public float transitionDuration = 0.1f;

    [Tooltip("애니메이션 재생 속도 (1 = 정상, 1.5 = 1.5배속. 엇박자 패턴 깎을 때 핵심!)")]
    public float animationSpeedMultiplier = 1.0f;

    [Tooltip("이 공격을 할 때 루트 모션(애니메이션 자체 이동)을 사용할 것인가?")]
    public bool useRootMotion = true;

    [Space(10)]
    [Header(" 3. 회전 및 유도 (Rotation & Tracking)")]
    public RotationTrackingType trackingType = RotationTrackingType.OnlyStartup;

    [Tooltip("보스가 플레이어를 바라보는 회전 속도 (높을수록 휙휙 돌아감)")]
    public float turnSpeed = 5f;

    [Tooltip("OnlyStartup 모드일 때, 애니메이션 진행도의 몇 % 시점에서 회전을 멈출 것인가? (예: 0.3이면 무기 내리찍기 직전)")]
    [Range(0f, 1f)] public float stopTrackingTime = 0.3f;

    [Space(10)]
    [Header(" 4. 전투 스탯 & 히트박스 타이밍 (Combat & Hitbox)")]
    public float damage = 20f;

    [Tooltip("소울류의 핵심: 플레이어의 강인도(Poise)를 깎는 수치")]
    public float poiseDamage = 10f;

    [Tooltip("이 공격을 하는 동안 보스가 경직에 면역이 되는가? (슈퍼아머)")]
    public bool hasHyperArmor = true;

    [Tooltip("히트박스(데미지 판정)가 켜지는 시점 (0~1)")]
    [Range(0f, 1f)] public float hitboxStartTime = 0.3f;
    [Tooltip("히트박스가 꺼지는 시점 (0~1)")]
    [Range(0f, 1f)] public float hitboxEndTime = 0.45f;

    [Space(10)]
    [Header(" 5. 콤보 및 상태 종료 (Combo & Exit)")]
    [Tooltip("선입력 혹은 자동 연계가 활성화되는 시점 (예: 0.6이면 후딜 구간부터 연계 가능)")]
    [Range(0f, 1f)] public float comboEnableTime = 0.6f;

    [Tooltip("이 애니메이션이 100% 끝나기 전에, 몇 % 시점에서 강제로 다음 생각(Decision)으로 넘어갈 것인가? (후딜레이 캔슬용)")]
    [Range(0f, 1f)] public float attackEndTime = 0.85f;

    [Tooltip("이 공격이 명중했거나 끝났을 때 확정적으로 이어나갈 다음 패턴 SO")]
    public BossAttack_SO_Data nextCombo;

    // 내부 관리용 변수 (인스펙터에 노출 안 됨)
    [HideInInspector] public float lastUsedTime = -999f;
}