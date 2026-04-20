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

    [Header("Stats & Phase")]
    private EnemyStats myStats; // 내 몸뚱이 스탯
    public float phase2Threshold = 0.5f;

    [Header("UI 연결")]
    public BossUIManager bossUI;
    public AnimatorOverrideController overrideController { get; protected set; }

    public BossStateMachine stateMachine { get; protected set; }

    
    public BossBaseState decisionState { get; protected set; }
    public List<BossActionOption> availableActions = new List<BossActionOption>();

    public BossBaseState idleState { get; protected set; }
    public BossBaseState chaseState { get; protected set; }
    public BossBaseState attackState { get; protected set; }
    public BossBaseState deadState { get; protected set; }
    public BossBaseState hitState { get; protected set; }
    public BossBaseState phaseChangeState { get; protected set; }

    public BossBaseState sleepState { get; protected set; }
    protected virtual void Awake()
    {
        stateMachine = new BossStateMachine();
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;
        }

        myStats = GetComponent<EnemyStats>();
        if (myStats != null)
        {
            //  스탯 방송국 구독! (피격당하면 내 함수를 실행해라!)
            myStats.OnTakeDamage += HandleDamageTaken;
            myStats.OnZeroHealth += HandleDeath;
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

    private void HandleDamageTaken()
    {
        // 2페이즈 전환 로직이 여기에 들어오면 완벽합니다!
        if (myStats.currentHealth <= myStats.maxHealth * 0.5f && currentPhase == 1)
        {
            Debug.Log(" 2페이즈 돌입!");
            if (phaseChangeState != null) stateMachine.ChangeState(phaseChangeState);
        }
    }

    private void HandleDeath()
    {
        Debug.Log(" 보스 사망!");
        stateMachine.ChangeState(deadState);
    }

    private void OnDestroy()
    {
        // ❗ 에러 방지를 위해 오브젝트가 파괴될 때 구독 취소
        if (myStats != null)
        {
            myStats.OnTakeDamage -= HandleDamageTaken;
            myStats.OnZeroHealth -= HandleDeath;
        }
    }

    public void WakeUpBoss()
    {
        // 자고 있을 때만 깨웁니다 (중복 실행 방지)
        if (stateMachine.CurrentState == sleepState)
        {

            //  추후 여기에 보스 체력바 UI 활성화 코드를 넣으면 완벽합니다!
            if (bossUI != null)
            {
                bossUI.ActivateBossUI();
            }
            // 수면 상태를 끝내고, 뇌(Decision)를 가동시킵니다.
            stateMachine.ChangeState(decisionState);
        }
    }
}