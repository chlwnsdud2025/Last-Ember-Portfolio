using System.Xml;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using static Attack_SO_Data;
using static UnityEditorInternal.VersionControl.ListControl;

public class PlayerController : MonoBehaviour
{
    public GameObject model;

    [Header("Move Settings")]
    public float moveSpeed = 5f;

    // 입력 시스템
    private InputSystem_Actions controls;
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public Vector2 mouseMoveInput;

    [Header("Look Settings")]
    public float mouseSensitivity = 10f; // 마우스 감도

    public Transform cameraTarget;
    public Transform cameraTransform;

    private float cameraPitch = 0f; // 상하 회전 각도
    private float cameraYaw = 0f;   // 좌우 회전 각도
    public float topClamp = 70f;    // 위로 쳐다볼 수 있는 최대 각도
    public float bottomClamp = -30f;// 아래로 내려다볼 수 있는 최대 각도

    [Header("Lock_On Settings")]
    public float lockOnRadius = 15f;     // 적을 찾는 반경
    public LayerMask enemyLayer;         // 적을 판별할 레이어
    [HideInInspector] public Transform currentTarget;
    [HideInInspector] public bool isLockedOn = false;

    [Header("Lock-On Switch")]
    public float switchCooldown = 0.3f; // 락온 변경 쿨타임
    private float currentSwitchCooldown = 0f;
    private bool hasFlicked = false;
    public LockCameraY cameraYLockScript;//락온시 카메라y축 고정할 스크립트

    [Header("Anim_Components")]
    public Animator animator;
    public CharacterController characterController;
    private AnimatorOverrideController overrideController;

    [Header("Combat & Dodge")]
    public WeaponController currentWeaponScript;
    public Attack_SO_Data currentWeapon;
    public Roll_SO_Data currentDodgeData;

    [HideInInspector] public bool canCombo;      // 현재 클릭 시 연속 공격 예약 가능 여부
    [HideInInspector] public bool canNextAttack; // 실제 다음 애니메이션으로 전환 가능한 시점
    [HideInInspector] public bool canCancel;     // 이동/회피로 현재 모션을 캔슬할 수 있는 여부
    [HideInInspector] public bool isInvincible = false; // 현재 구르기 무적 상태인지 확인하는 플래그
    
    //중력
    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float gravityMultiplier = 2f;
    [HideInInspector] public float verticalVelocity;

    [HideInInspector] public Vector3 currentRollDir = Vector3.zero;

    [Header("Input States")]
    [HideInInspector] public bool isSprintButtonHeld = false;
    [HideInInspector] public bool isWalkButtonHeld = false;

    public bool isDead = false;



    public PlayerStats stats { get; private set; }

    // 상태 머신 (FSM)
    public StateMachine stateMachine { get; private set; }
    public IdleState idleState { get; private set; }
    public MoveState moveState { get; private set; }
    public AttackState attackState { get; private set; }
    public DodgeState dashState { get; private set; }
    public SprintState sprintState { get; private set; }
    public WalkState walkState { get; private set; }
    public HitState hitState { get; private set; }

    public DeadState deadState { get; private set; }
    public DrinkState drinkState { get; private set; }

    private void Awake()
    {
        controls = new InputSystem_Actions();
        stateMachine = new StateMachine();

        idleState = new IdleState(this, stateMachine);
        moveState = new MoveState(this, stateMachine);
        attackState = new AttackState(this, stateMachine);
        dashState = new DodgeState(this, stateMachine);
        sprintState = new SprintState(this, stateMachine);
        walkState = new WalkState(this, stateMachine);
        hitState = new HitState(this, stateMachine);
        deadState = new DeadState(this, stateMachine);
        drinkState = new DrinkState(this, stateMachine);

        // 단발성 액션 이벤트
        controls.PlayerMovement.Attack.performed += ctx => stateMachine.CurrentState.OnAttackInput();
        controls.PlayerMovement.Dodge.performed += ctx => stateMachine.CurrentState.OnDashInput();
        controls.PlayerMovement.LockOn.performed += ctx => ToggleLockOn();
        controls.PlayerMovement.Potion.performed += ctx => stateMachine.CurrentState.OnPotionInput();

        // 런타임에 애니메이션 클립을 동적으로 갈아끼우기 위한 오버라이드 컨트롤러 세팅
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;

        stats = GetComponent<PlayerStats>(); // 스텟 연결
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (currentWeapon != null) EquipWeapon(currentWeapon);

        stateMachine.Initialize(idleState);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        moveInput = controls.PlayerMovement.Move.ReadValue<Vector2>();
        mouseMoveInput = controls.PlayerMovement.Look.ReadValue<Vector2>();

        isSprintButtonHeld = controls.PlayerMovement.Sprint.IsPressed();
        isWalkButtonHeld = controls.PlayerMovement.Walk.IsPressed();

        if (isLockedOn && currentTarget != null)
        {
            if (currentSwitchCooldown > 0)
            {
                currentSwitchCooldown -= Time.deltaTime;
            }

            // 1. 임계값(Threshold)을 대폭 상향합니다. 마우스 감도에 따라 10f ~ 30f 사이로 조절하세요!
            float flickThreshold = 30.0f;

            // 2. 쿨타임이 끝났고, 아직 방아쇠를 당기지 않은 상태일 때만 검사
            if (currentSwitchCooldown <= 0 && !hasFlicked)
            {
                // 우측으로 강하게 튕김
                if (mouseMoveInput.x > flickThreshold)
                {
                    SwitchLockOnTarget(1);
                    hasFlicked = true; // 방아쇠 당김! (연속 발동 잠금)
                }
                // 좌측으로 강하게 튕김
                else if (mouseMoveInput.x < -flickThreshold)
                {
                    SwitchLockOnTarget(-1);
                    hasFlicked = true; // 방아쇠 당김!
                }
            }

            // 3.  [핵심] 마우스 움직임이 멈추면(0에 가까워지면) 다시 튕길 수 있도록 방아쇠를 풀어줍니다.
            if (Mathf.Abs(mouseMoveInput.x) < 2.0f)
            {
                hasFlicked = false;
            }
        }

        stateMachine.Update();
    }


    // 카메라 회전 및 움직임 ( 락온 부분 이해 필요! )
    private void LateUpdate()
    {
        if (cameraTransform == null || cameraTarget == null) return;

        cameraTarget.position = transform.position + Vector3.up * 2f;

        // 락온 카메라 처리
        if (isLockedOn && currentTarget != null)
        {
            Vector3 dirToTarget = currentTarget.position - cameraTarget.position;

            // Y축 높이 차이를 무시하여 상하 회전 방지 (위치 고정)
            dirToTarget.y = 0f;

            float distanceToTarget = dirToTarget.magnitude;

            // 거리가 0이 되어 LookRotation이 에러를 뿜는 것 방지
            if (dirToTarget != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
                float angleToTarget = Vector3.Angle(cameraTarget.forward, dirToTarget);

                float currentRotationSpeed = 15f;
                if (distanceToTarget < 1.0f)
                {
                    currentRotationSpeed = Mathf.Lerp(2f, 15f, distanceToTarget / 2.0f);
                }

                if (angleToTarget < 10.0f)
                {
                    currentRotationSpeed *= (angleToTarget / 5.0f);
                }

                cameraTarget.rotation = Quaternion.Slerp(cameraTarget.rotation, targetRot, currentRotationSpeed * Time.deltaTime);
            }

            Vector3 euler = cameraTarget.rotation.eulerAngles;
            cameraYaw = euler.y;
            cameraPitch = euler.x > 180 ? euler.x - 360 : euler.x;
        }
        else
        {
            // 자유 카메라 처리 (기존과 동일)
            float lookX = mouseMoveInput.x * mouseSensitivity * 0.1f;
            float lookY = mouseMoveInput.y * mouseSensitivity * 0.1f;

            cameraYaw += lookX;
            cameraPitch -= lookY;
            cameraPitch = Mathf.Clamp(cameraPitch, bottomClamp, topClamp);

            cameraTarget.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
        }
    }

    //공격 및 구르기의 모션 데이터 덮어쓰기
    public void EquipWeapon(Attack_SO_Data newData)
    {
        currentWeapon = newData;

        
        for (int i = 0; i < newData.comboAttacks.Length; i++)
        {
            overrideController[$"Attack{i + 1}"] = newData.comboAttacks[i].clip;
        }

        if (currentDodgeData != null)
        {
            overrideController["Roll"] = currentDodgeData.clip;
        }
    }


    // 루트 모션 이동량 제어
    private void OnAnimatorMove()
    {
        if (animator.applyRootMotion && characterController != null)
        {
            // 1. 락온 중 구르기 상태일 때 (기존 유지)
            if (stateMachine.CurrentState == dashState && currentRollDir != Vector3.zero)
            {
                Vector3 delta = animator.deltaPosition;
                float moveDistance = new Vector3(delta.x, 0f, delta.z).magnitude;

                Vector3 finalMove = currentRollDir * moveDistance;
                MoveCharacter(finalMove);
            }
            else
            {
                // 2. 평소 (공격, 걷기 등)
                Vector3 rootMotion = animator.deltaPosition;
                rootMotion.y = 0f;

                // --- [프롬소프트웨어식: 하이브리드 감쇄 & 부스트 루트 모션] ---
                if (isLockedOn && currentTarget != null && stateMachine.CurrentState == attackState)
                {
                    float dist = Vector3.Distance(transform.position, currentTarget.position);

                    // 수치 튜닝 포인트 (무기 리치에 따라 조절하세요)
                    float stopDistance = 1.0f;        // 1. 감쇄: 이 거리보다 가까우면 덜덜거림 방지를 위해 전진 정지
                    float perfectHitDistance = 2.0f;  // 2. 워핑: 이 거리보다 멀면 적에게 빠르게 훅! 다가감

                    // 애니메이션이 '앞으로' 가려는 순수 전진력 추출
                    float forwardMagnitude = Vector3.Dot(rootMotion, transform.forward);

                    // 뒤로 물러나는 모션이 아니라 전진하는 모션일 때만 발동
                    if (forwardMagnitude > 0)
                    {
                        Vector3 forwardVector = transform.forward * forwardMagnitude;

                        if (dist < stopDistance)
                        {
                            // [하이브리드 루트 모션] 적과 너무 겹치면 전진력을 깎아내어 충돌 지터링 완전 방지
                            // 거리가 0에 가까워질수록 damping이 0이 되어 전진력이 완전히 사라짐
                            float damping = dist / stopDistance;
                            rootMotion -= forwardVector * (1f - damping);
                        }
                        else if (dist > perfectHitDistance)
                        {
                            // [타겟 워핑 / 부스트] 멀리 있으면 루트 모션을 강제로 증폭시켜 거리를 좁힘
                            // 최대 2.5배까지만 증폭하도록 제한 (순간이동처럼 보이는 것 방지)
                            float boostMultiplier = Mathf.Clamp(dist / perfectHitDistance, 1f, 1.2f);

                            // 원래의 전진력을 빼고, 증폭된 전진력으로 덮어씌움
                            rootMotion -= forwardVector;
                            rootMotion += forwardVector * boostMultiplier;
                        }
                    }
                }
                // -------------------------------------------------------------

                MoveCharacter(rootMotion);
            }
        }
    }

    //락온 끄기
    public void ClearLockOn()
    {
        isLockedOn = false;
        currentTarget = null;
        cameraYLockScript.isLockedOn = false;
    }

    // 락온 켜기
    private void ToggleLockOn()
    {
        if (isLockedOn)
        {
            ClearLockOn();
            return;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, lockOnRadius, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (Collider col in colliders)
        {
            Vector3 dirToTarget = (col.transform.position - cameraTarget.position).normalized;
            float dot = Vector3.Dot(cameraTarget.forward, dirToTarget);

            if (dot > 0.5f)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;

                    EnemyController targetEnemy = col.GetComponent<EnemyController>();
                    BossBaseController targetBoss = col.GetComponent<BossBaseController>();

                    if (targetEnemy != null && targetEnemy.lockOnPoint != null)
                    {
                        bestTarget = targetEnemy.lockOnPoint;
                    }
                    else if (targetBoss != null && targetBoss.lockOnPoint != null)
                    {
                        bestTarget = targetBoss.lockOnPoint;
                    }
                    else
                    {
                        bestTarget = col.transform;
                    }
                }
            }
        }

        //  타겟을 확실히 찾았을 때만 모든 변수를 true로 만듭니다.
        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            isLockedOn = true;

            if (cameraYLockScript != null)
            {
                cameraYLockScript.isLockedOn = true;
                Debug.Log("카메라 Y축 고정 스크립트 ON!"); // 디버그 로그 추가
            }
        }
        else
        {
            ClearLockOn(); // 허공에 눌렀을 때 확실하게 초기화
        }
    }

    //락온 바꾸기
    private void SwitchLockOnTarget(int direction)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockOnRadius, enemyLayer);

        float closestAngle = Mathf.Infinity;
        Transform newTarget = null;

        // 현재 쳐다보고 있는 방향
        Vector3 dirToCurrent = (currentTarget.position - cameraTarget.position).normalized;

        foreach (Collider col in colliders)
        {
            EnemyController targetEnemy = col.GetComponent<EnemyController>();
            BossBaseController targetBoss = col.GetComponent<BossBaseController>();
            if (targetEnemy == null && targetBoss == null) continue;


            //Transform potentialTarget = targetEnemy.lockOnPoint != null ? targetEnemy.lockOnPoint : col.transform;
            Transform potentialTarget = col.transform; // 기본값

            if (targetEnemy != null && targetEnemy.lockOnPoint != null)
                potentialTarget = targetEnemy.lockOnPoint;
            else if (targetBoss != null && targetBoss.lockOnPoint != null)
                potentialTarget = targetBoss.lockOnPoint;
            // 락온 포인트가 있으면 쓰고, 없으면 전체 몸통 사용

            // 자기 자신(현재 타겟)은 건너뜀
            if (potentialTarget == currentTarget) continue;

            // 판별할 적을 향한 방향
            Vector3 dirToPotential = (potentialTarget.position - cameraTarget.position).normalized;

            // 1. 카메라 시야각 안에 있는지 확인 (뒤에 있는 적 무시)
            if (Vector3.Dot(cameraTarget.forward, dirToPotential) < 0.2f) continue;

            // 2. 이 적이 현재 타겟을 기준으로 '오른쪽'에 있는지 '왼쪽'에 있는지 판별
            // 카메라의 Right 벡터와 내적(Dot)을 구하면 우측일 때 양수, 좌측일 때 음수가 나옵니다.
            float dotRight = Vector3.Dot(cameraTarget.right, dirToPotential - dirToCurrent);
            bool isRight = dotRight > 0;

            // 3. 사용자가 입력한 방향(우측/좌측)과 적의 위치가 일치하는지 확인
            if ((direction == 1 && isRight) || (direction == -1 && !isRight))
            {
                // 4. 방향이 맞는 적들 중에서 '현재 타겟과 가장 각도가 가까운' 적을 찾음
                float angle = Vector3.Angle(dirToCurrent, dirToPotential);
                if (angle < closestAngle)
                {
                    closestAngle = angle;
                    newTarget = potentialTarget;
                }
            }
        }

        // 새로운 타겟을 찾았다면 변경!
        if (newTarget != null)
        {
            currentTarget = newTarget;
            currentSwitchCooldown = switchCooldown; // 쿨타임 초기화
            // Debug.Log("락온 타겟 변경 완료!");
        }
    }


    //중력 적용 후 움직이기
    public void MoveCharacter(Vector3 horizontalMotion)
    {
        // 중력 계산
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0.0f) verticalVelocity = -2f; 
        }
        else
        {
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime; 
        }

        // 수평 이동(걷기/루트모션) + 수직 이동(중력) 합치기
        horizontalMotion.y += verticalVelocity * Time.deltaTime;

        // 최종 이동 실행
        characterController.Move(horizontalMotion);
    }

    public void TakeDamage(float damage)
    {
        // 1. 무적 판정 (구르기 중)
        if (isInvincible)
        {
            Debug.Log("회피 성공! (무적 프레임)");
            return;
        }
        if (isDead)
        {
            return;
        }

        if (stats != null)
        {
            isDead = stats.TakeDamage(damage);
        }
        // 3. 상태 전환
        if (isDead)
        {
            stateMachine.ChangeState(deadState);
        }
        else
        {
            stateMachine.ChangeState(hitState);
        }
    }
    private void OnEnable() { controls.Enable(); }
    private void OnDisable() { controls.Disable(); }
}