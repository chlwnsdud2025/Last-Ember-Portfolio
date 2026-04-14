using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public CharacterController characterController;

    [Header("Aggro Settings")]
    public float detectionRadius = 10f; // 감지 반경
    [Range(0, 360)]
    public float viewAngle = 120f;      // 시야각
    public LayerMask playerLayer;       // 플레이어 레이어
    public LayerMask obstacleLayer;     // 감지 방해용 장애물 레이어


    [Header("Attack Settings")]
    public float AttackRangge = 1f;

    [HideInInspector] public Transform target;

    [Header("Move Settings")]
    public float moveSpeed = 1f;

    [Header("Combat Settings")]
    public float attackRange = 0.1f;     // 공격 사거리
    public float strafeRange = 2f;
    public float strafeSpeed = 0.3f;     
    public float attackCooldown = 3f;    // 공격 쿨타임 
    [HideInInspector] public float lastAttackTime = -999f; // 마지막으로 공격한 시간

    //중력
    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float gravityMultiplier = 2f;
    [HideInInspector] public float verticalVelocity;


    public float currentHealth = 10f;
    [Header("Weapon data")]
    public EnemyWeaponController currentWeaponScript;



    // 상태 머신
    public EnemyStateMachine stateMachine { get; private set; }
    public EnemyIdleState idleState { get; private set; }
    public EnemyChaseState chaseState { get; private set; }
    public EnemyStrafeState strafeState { get; private set; }
    public EnemyAttackState attackState { get; private set; }
    public EnemyHitState hitsState { get; private set; }


    private void Awake()
    {
        stateMachine = new EnemyStateMachine();
        idleState = new EnemyIdleState(this, stateMachine);
        chaseState = new EnemyChaseState(this, stateMachine);
        strafeState = new EnemyStrafeState(this, stateMachine);
        attackState = new EnemyAttackState(this, stateMachine);
        hitsState = new EnemyHitState(this, stateMachine);

    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.Update();
    }

   
    private void OnDrawGizmosSelected()//기즈모 잘 모르겠음
    {
        // 1. 감지 반경 원 그리기 (흰색)
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 2. 시야각 부채꼴 선 그리기 (빨간색)
        Gizmos.color = Color.red;

        // 왼쪽 시야 한계선
        Vector3 leftBoundary = DirFromAngle(transform.eulerAngles.y, -viewAngle / 2);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRadius);

        // 오른쪽 시야 한계선
        Vector3 rightBoundary = DirFromAngle(transform.eulerAngles.y, viewAngle / 2);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // 각도를 방향 벡터로 변환해주는 헬퍼 함수 (기즈모 용도)
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

    public void TakeDamage(float damage)
    {
        //currentHealth -= damage;

        if (currentHealth <= 0)
        {
            // 체력이 0 이하면 죽음 상태로 (구현 예정)
            // stateMachine.ChangeState(deadState);
        }
        else
        {
            // 체력이 남아있다면, 현재 무슨 행동을 하고 있었든 간에 피격 상태로 강제 전환!
            stateMachine.ChangeState(hitsState);
        }
    }
}
