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

    [Header("Hitbox System")]
    public BossHitboxManager hitboxManager;

    [Header("UI 연결")]
    public BossUIManager bossUI;
    public AnimatorOverrideController overrideController { get; protected set; }

    public BossStateMachine stateMachine { get; protected set; }

    [Header("Reset System")]
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;

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

        // 1. 처음 배치된 위치와 바라보는 방향을 기억해 둡니다.
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // 2. 화톳불 리셋 이벤트 구독
        Bonfire.OnBonfireRest += ResetBoss;
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

        Bonfire.OnBonfireRest -= ResetBoss;
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

    public virtual void ResetBoss()
    {
        Debug.Log("화톳불 리셋: 보스 초기화 진행!");

        // 1. 위치 원상복구 (CharacterController 강제 이동을 위해 잠시 끄기)
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        if (cc != null) cc.enabled = true; // 죽으면서 꺼진 컨트롤러도 복구됨

        // 2. 죽으면서 꺼진 콜라이더 및 락온 포인트 복구
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
        if (lockOnPoint != null) lockOnPoint.gameObject.SetActive(true);

        // 3. 체력 풀회복 및 페이즈 초기화
        if (myStats != null)
        {
            myStats.currentHealth = myStats.maxHealth;
        }
        currentPhase = 1;

        // 4. 보스 UI 끄기 (다시 트리거를 밟을 때 켜집니다)
        if (bossUI != null && bossUI.bossUIContainer != null)
        {
            bossUI.bossUIContainer.SetActive(false);
        }

        // 5. 애니메이션 강제 정지 및 상태 머신을 '수면 상태'로 되돌림
        animator.Play("Idle", 0, 0f);
        animator.applyRootMotion = false; // 죽을 때 꺼진 루트모션 플래그 원상복구

        if (sleepState != null)
        {
            stateMachine.ChangeState(sleepState);
        }
    }
}