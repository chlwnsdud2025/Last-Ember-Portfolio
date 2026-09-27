using UnityEngine;

public enum AttackMethod
{
    Melee,       // 내 몸의 콜라이더를 켜는 타격
    SpawnPrefab  // 투사체, 충격파, 장판 소환
}

public enum HitboxType
{
    None,
    RightHandWeapon,
    LeftHandWeapon,
    RightFoot,
    LeftFoot,
    Body,
    Tail
}

public enum RotationTrackingType
{
    None,               // 아예 안 쳐다봄 (광역기 등)
    OnlyStartup,        // 선딜레이 구간에만 쳐다봄 (가장 일반적인 소울류 공격)
    Continuous          // 공격 내내 쳐다봄 (레이저 빔, 휠윈드 등)
}

[CreateAssetMenu(fileName = "New Boss Attack", menuName = "Boss/AAA Attack Data")]
public class BossAttack_SO_Data : ScriptableObject
{
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

    public AnimationClip attackClip;

    [Tooltip("애니메이션 전환(CrossFade)에 걸리는 부드러운 시간 (보통 0.1 ~ 0.2)")]
    public float transitionDuration = 0.1f;

    [Tooltip("애니메이션 재생 속도 (1 = 정상, 1.5 = 1.5배속. 엇박자 패턴 깎을 때 핵심!)")]
    public float animationSpeedMultiplier = 1.0f;

    [Tooltip("이 공격을 할 때 루트 모션(애니메이션 자체 이동)을 사용할 것인가?")]
    public bool useRootMotion = true;

    public RotationTrackingType trackingType = RotationTrackingType.OnlyStartup;

    [Tooltip("보스가 플레이어를 바라보는 회전 속도 (높을수록 휙휙 돌아감)")]
    public float turnSpeed = 5f;

    [Tooltip("OnlyStartup 모드일 때, 애니메이션 진행도의 몇 % 시점에서 회전을 멈출 것인가? (예: 0.3이면 무기 내리찍기 직전)")]
    [Range(0f, 1f)] public float stopTrackingTime = 0.3f;

    public float damage = 20f;

    [Tooltip("소울류의 핵심: 플레이어의 강인도(Poise)를 깎는 수치")]
    public float poiseDamage = 10f;

    [Tooltip("이 공격을 하는 동안 보스가 경직에 면역이 되는가? (슈퍼아머)")]
    public bool hasHyperArmor = true;

    [Tooltip("이 공격의 종류 (직접 때리기 vs 프리팹 소환하기)")]
    public AttackMethod attackMethod = AttackMethod.Melee;

    [Tooltip("Melee 방식일 때 켤 히트박스 부위")]
    public HitboxType targetHitbox;

    [Tooltip("SpawnPrefab 방식일 때 소환할 투사체/장판 프리팹")]
    public GameObject attackPrefab;
    [Tooltip("보스를 기준으로 어느 위치에 소환할 것인가 (예: 0, 0, 1 은 보스 앞 1m)")]
    public Vector3 spawnOffset = new Vector3(0, 0, 1f);
    [Tooltip("체크하면 플레이어 발 밑에 바로 소환됩니다.")]
    public bool spawnOnTarget = false;

    [Tooltip("히트박스가 켜지거나 프리팹이 소환되는 시작 시점 (0~1)")]
    [Range(0f, 1f)] public float hitboxStartTime = 0.3f;
    [Tooltip("히트박스가 꺼지는 시점 (0~1) (SpawnPrefab일 경우 무시됨)")]
    [Range(0f, 1f)] public float hitboxEndTime = 0.45f;

    [Tooltip("선입력 혹은 자동 연계가 활성화되는 시점 (예: 0.6이면 후딜 구간부터 연계 가능)")]
    [Range(0f, 1f)] public float comboEnableTime = 0.6f;

    [Tooltip("이 애니메이션이 100% 끝나기 전에, 몇 % 시점에서 강제로 다음 생각(Decision)으로 넘어갈 것인가? (후딜레이 캔슬용)")]
    [Range(0f, 1f)] public float attackEndTime = 0.85f;

    [Tooltip("이 공격이 명중했거나 끝났을 때 확정적으로 이어나갈 다음 패턴 SO")]
    public BossAttack_SO_Data nextCombo;

    // 내부 관리용 변수 (인스펙터에 노출 안 됨)
    [HideInInspector] public float lastUsedTime = -999f;
}