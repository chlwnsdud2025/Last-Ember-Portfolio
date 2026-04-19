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

    public BossStateMachine stateMachine { get; protected set; }

    
    public BossBaseState decisionState { get; protected set; }
    public List<BossActionOption> availableActions = new List<BossActionOption>();

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


    public void ReturnToDecision()
    {
        if (stateMachine.CurrentState != decisionState && decisionState != null)
        {
            stateMachine.ChangeState(decisionState);
        }
    }

    public void MoveBoss(Vector3 moveVector)
    {
        transform.position += moveVector;
    }

    // 수정 전: public void RotateTowardsTarget()
    // 수정 후: speed 값을 넘겨받을 수 있게 오버로딩 (기본값은 기존 rotationSpeed)
    public void RotateTowardsTarget(float overrideSpeed = -1f)
    {
        if (target == null) return;

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        dirToTarget.y = 0;

        if (dirToTarget != Vector3.zero)
        {
            // overrideSpeed가 안 들어오면(-1) 기본 속도, 들어오면 그 속도를 씁니다.
            float finalSpeed = (overrideSpeed > 0f) ? overrideSpeed : rotationSpeed;

            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, finalSpeed * Time.deltaTime);
        }
    }
}