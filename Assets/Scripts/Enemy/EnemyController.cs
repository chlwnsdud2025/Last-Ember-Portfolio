using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public CharacterController characterController;

    [Header("Aggro Settings")]
    public float detectionRadius = 10f; // 탐지 반경
    [Range(0, 360)]
    public float viewAngle = 120f;      // 시야각
    public LayerMask playerLayer;       // 플레이어 레이어
    public LayerMask obstacleLayer;     // 장애물 레이어

    [HideInInspector] public Transform target;

    [Header("Move Settings")]
    public float moveSpeed = 1f;

    [Header("Combat Settings")]
    public float strafeRange = 3f;
    public float strafeSpeed = 0.3f;
    public float attackCooldown = 3f;    // 공격 쿨타임 
    [HideInInspector] public float lastAttackTime = -999f; // 마지막 공격 시간

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float gravityMultiplier = 2f;
    [HideInInspector] public float verticalVelocity;

    [Header("Weapons / Hitboxes")]
    public EnemyWeaponController[] weapons;

    [Header("Attack Patterns (SO)")]
    public EnemyAttack_SO_Data[] attackPatterns;

    private EnemyStats myStats;

    [Header("Targeting")]
    public Transform lockOnPoint;

    private AnimatorOverrideController overrideController;
    // 상태들
    public EnemyStateMachine stateMachine { get; private set; }
    public EnemyIdleState idleState { get; private set; }
    public EnemyChaseState chaseState { get; private set; }
    public EnemyStrafeState strafeState { get; private set; }
    public EnemyHitState hitsState { get; private set; }
    public EnemyDeadState deadState { get; private set; }

    // ❗ 다중 공격 상태 배열
    public EnemyAttackState[] attackStates { get; private set; }

    private void Awake()
    {
        stateMachine = new EnemyStateMachine();
        idleState = new EnemyIdleState(this, stateMachine);
        chaseState = new EnemyChaseState(this, stateMachine);
        strafeState = new EnemyStrafeState(this, stateMachine);
        hitsState = new EnemyHitState(this, stateMachine);
        deadState = new EnemyDeadState(this, stateMachine);

        
        if (animator.runtimeAnimatorController != null)
        {
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;
        }

        if (attackPatterns != null && attackPatterns.Length > 0)
        {
            attackStates = new EnemyAttackState[attackPatterns.Length];

            for (int i = 0; i < attackPatterns.Length; i++)
            {
                // 1. 에너미 애니메이터에 있는 가짜 노드 이름 (예: "Attack0", "Attack1")
                string stateName = $"Attack{i}";

                // 2. SO에 클립이 등록되어 있다면 런타임에 갈아끼웁니다! (플레이어와 동일한 방식)
                if (attackPatterns[i].clip != null)
                {
                    overrideController[stateName] = attackPatterns[i].clip;
                }

                // 3. State를 생성할 때, "Attack0", "Attack1" 같은 재생할 노드 이름을 같이 넘겨줍니다.
                attackStates[i] = new EnemyAttackState(this, stateMachine, attackPatterns[i], stateName);
            }
        }

        myStats = GetComponent<EnemyStats>();
        if (myStats != null)
        {
            myStats.OnTakeDamage += HandleDamageTaken;
            myStats.OnZeroHealth += HandleDeath;
        }
    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.Update();
    }

    public EnemyWeaponController GetWeapon(int index)
    {
        if (weapons != null && index >= 0 && index < weapons.Length)
        {
            return weapons[index];
        }
        Debug.LogWarning($"[EnemyController] {index}번 무기가 할당되지 않았습니다!");
        return null;
    }

    //사거리 그리기
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Vector3 leftBoundary = DirFromAngle(transform.eulerAngles.y, -viewAngle / 2);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRadius);

        Vector3 rightBoundary = DirFromAngle(transform.eulerAngles.y, viewAngle / 2);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, strafeRange);
    }

    //사거리 그리기위한 앵글 계산
    private Vector3 DirFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public void MoveEnemy(Vector3 horizontal)
    {
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0.0f) verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }

        horizontal.y += verticalVelocity * Time.deltaTime;
        characterController.Move(horizontal);
    }

    private void OnAnimatorMove()
    {
        if (animator.applyRootMotion && characterController != null && characterController.enabled)
        {
            Vector3 rootMotion = animator.deltaPosition;
            rootMotion.y = 0f;

            
            if (target != null && stateMachine.enemyCurrentState is EnemyAttackState)
            {
                float dist = Vector3.Distance(transform.position, target.position);
                float stopDistance = 1.0f;

                float forwardMagnitude = Vector3.Dot(rootMotion, transform.forward);

                if (forwardMagnitude > 0)
                {
                    Vector3 forwardVector = transform.forward * forwardMagnitude;

                    if (dist < stopDistance)
                    {
                        float damping = dist / stopDistance;
                        rootMotion -= forwardVector * (1f - damping);
                    }
                }
            }

            if (!characterController.isGrounded)
            {
                verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
            }
            else if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            rootMotion.y = verticalVelocity * Time.deltaTime;
            characterController.Move(rootMotion);
        }
    }

    public EnemyAttackState GetBestAttack(float distanceToPlayer)
    {
        if (attackPatterns == null || attackPatterns.Length == 0) return null;

        System.Collections.Generic.List<int> validIndices = new System.Collections.Generic.List<int>();
        int totalWeight = 0;

        // 1. 발동 가능한 공격들만 추려내기 (거리 검사 + 개별 쿨타임 검사)
        for (int i = 0; i < attackPatterns.Length; i++)
        {
            // 거리 조건 확인
            bool isDistanceValid = distanceToPlayer >= attackPatterns[i].minAttackRange &&
                                   distanceToPlayer <= attackPatterns[i].maxAttackRange;

            // 이 스킬의 개별 쿨타임이 다 돌았는가?
            bool isSkillReady = Time.time >= attackStates[i].lastUsedTime + attackPatterns[i].cooldown;

            // 거리도 맞고 쿨타임도 돌았다면 후보에 추가!
            if (isDistanceValid && isSkillReady)
            {
                validIndices.Add(i);
                totalWeight += attackPatterns[i].priorityWeight;
            }
        }

        // 현재 쓸 수 있는 공격이 하나도 없다면 null 반환
        if (validIndices.Count == 0) return null;

        // 2. 가중치(Priority Weight) 기반 룰렛 돌리기
        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (int index in validIndices)
        {
            cumulativeWeight += attackPatterns[index].priorityWeight;
            if (randomValue < cumulativeWeight)
            {
                //  기존에 있던 attackCooldown 덮어쓰기 삭제! (개별 쿨타임만 돕니다)
                return attackStates[index];
            }
        }

        return attackStates[validIndices[0]];
    }

    private void HandleDamageTaken()
    {
        if (myStats.currentHealth <= 0) return;
        stateMachine.ChangeState(hitsState);
    }

    private void HandleDeath()
    {
        stateMachine.ChangeState(deadState);
    }

    private void OnDestroy()
    {
        // 에러 방지를 위해 오브젝트가 파괴될 때 구독 취소
        if (myStats != null)
        {
            myStats.OnTakeDamage -= HandleDamageTaken;
            myStats.OnZeroHealth -= HandleDeath;
        }
    }
}