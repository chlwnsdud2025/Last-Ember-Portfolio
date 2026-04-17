using System.Collections.Generic; 
using UnityEngine;

public abstract class BossBaseController : MonoBehaviour
{
    [Header("Core Components")]
    public Animator animator;
    public Transform lockOnPoint;
    [HideInInspector] public Transform target;

    [Header("Base Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public int currentPhase = 1;

    public AnimatorOverrideController overrideController { get; protected set; }

    // 상태 머신
    public BossStateMachine stateMachine { get; protected set; }

    
    public BossBaseState decisionState { get; protected set; }
    public List<BossActionOption> availableActions = new List<BossActionOption>();

    // 상태 슬롯들
    public BossBaseState idleState { get; protected set; }
    public BossBaseState chaseState { get; protected set; }
    public BossBaseState attackState { get; protected set; }
    public BossBaseState deadState { get; protected set; }
    public BossBaseState hitState { get; protected set; }

    protected virtual void Awake()
    {
        stateMachine = new BossStateMachine();
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;
        }
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    protected virtual void Update()
    {
        stateMachine.Update();
    }

    // ---------------------------------------------------
    //  행동을 마치고 다시 생각하러 돌아가는 핵심 트리거
    // ---------------------------------------------------
    public void ReturnToDecision()
    {
        if (stateMachine.CurrentState != decisionState && decisionState != null)
        {
            stateMachine.ChangeState(decisionState);
        }
    }

    // --- 공용 이동 로직 ---
    public void MoveBoss(Vector3 moveVector)
    {
        transform.position += moveVector;
    }

    public void RotateTowardsTarget()
    {
        if (target == null) return;

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        dirToTarget.y = 0;

        if (dirToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}