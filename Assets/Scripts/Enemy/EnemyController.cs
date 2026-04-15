using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public CharacterController characterController;

    [Header("Aggro Settings")]
    public float detectionRadius = 10f; // ���� �ݰ�
    [Range(0, 360)]
    public float viewAngle = 120f;      // �þ߰�
    public LayerMask playerLayer;       // �÷��̾� ���̾�
    public LayerMask obstacleLayer;     // ���� ���ؿ� ��ֹ� ���̾�


    [Header("Attack Settings")]
    public float AttackRangge = 1f;

    [HideInInspector] public Transform target;

    [Header("Move Settings")]
    public float moveSpeed = 1f;

    [Header("Combat Settings")]
    public float attackRange = 0.1f;     // ���� ��Ÿ�
    public float strafeRange = 2f;
    public float strafeSpeed = 0.3f;     
    public float attackCooldown = 3f;    // ���� ��Ÿ�� 
    [HideInInspector] public float lastAttackTime = -999f; // ���������� ������ �ð�

    //�߷�
    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float gravityMultiplier = 2f;
    [HideInInspector] public float verticalVelocity;


    [Header("Weapon data")]
    public EnemyWeaponController currentWeaponScript;



    // ���� �ӽ�
    public EnemyStateMachine stateMachine { get; private set; }
    public EnemyIdleState idleState { get; private set; }
    public EnemyChaseState chaseState { get; private set; }
    public EnemyStrafeState strafeState { get; private set; }
    public EnemyAttackState attackState { get; private set; }
    public EnemyHitState hitsState { get; private set; }
    public EnemyDeadState deadState { get; private set; }


    private void Awake()
    {
        stateMachine = new EnemyStateMachine();
        idleState = new EnemyIdleState(this, stateMachine);
        chaseState = new EnemyChaseState(this, stateMachine);
        strafeState = new EnemyStrafeState(this, stateMachine);
        attackState = new EnemyAttackState(this, stateMachine);
        hitsState = new EnemyHitState(this, stateMachine);
        deadState = new EnemyDeadState(this, stateMachine);

    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.Update();
    }

   
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
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }


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
        // 루트 모션이 켜져 있을 때만 애니메이션의 이동 값을 CharacterController에 적용
        if (animator.applyRootMotion && characterController != null)
        {
            Vector3 rootMotion = animator.deltaPosition;
            rootMotion.y = 0f; // 수평 이동만 분리

            // --- [프롬소프트웨어식: 적(Enemy) 타겟 워핑 & 감쇄 로직] ---
            if (target != null && stateMachine.enemyCurrentState == attackState)
            {
                float dist = Vector3.Distance(transform.position, target.position);

                // 무기 리치나 몬스터 크기에 따라 조절하세요
                float stopDistance = 2.0f;       // 1. 플레이어와 겹치면 전진 차단 (지터링 방지)
                float perfectHitDistance = 2.0f; // 2. 이 거리보다 멀면 플레이어를 향해 미끄러지듯 돌진

                float forwardMagnitude = Vector3.Dot(rootMotion, transform.forward);

                // 뒤로 빠지는 모션이 아닌, 앞으로 전진하는 모션일 때만 발동
                if (forwardMagnitude > 0)
                {
                    Vector3 forwardVector = transform.forward * forwardMagnitude;

                    if (dist < stopDistance)
                    {
                        // [감쇄] 플레이어와 너무 가깝다면 루트 모션의 전진력을 깎아냄
                        float damping = dist / stopDistance;
                        rootMotion -= forwardVector * (1f - damping);
                    }
                    //else if (dist > perfectHitDistance)
                    //{
                    //    // [몬스터 전용 부스트] 거리가 멀면 루트 모션 증폭
                    //    float boostMultiplier = Mathf.Clamp(dist / perfectHitDistance, 1f, 1.2f);

                    //    rootMotion -= forwardVector;
                    //    rootMotion += forwardVector * boostMultiplier;
                    //}
                }
            }
            // -------------------------------------------------------------

            // 중력 적용 (기존 로직 유지)
            if (!characterController.isGrounded)
            {
                verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
            }
            else if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            // 수평 이동(워핑 적용됨) + 수직 이동(중력) 합치기
            rootMotion.y = verticalVelocity * Time.deltaTime;

            // 최종 이동 실행
            characterController.Move(rootMotion);
        }
    }


}
