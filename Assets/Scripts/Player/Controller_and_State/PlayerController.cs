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

    // 상태 머신 (FSM)
    public StateMachine stateMachine { get; private set; }
    public IdleState idleState { get; private set; }
    public MoveState moveState { get; private set; }
    public AttackState attackState { get; private set; }
    public DodgeState dashState { get; private set; }
    public SprintState sprintState { get; private set; }
    public WalkState walkState { get; private set; }
    public HitState hitState { get; private set; }

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


        // 단발성 액션 이벤트
        controls.PlayerMovement.Attack.performed += ctx => stateMachine.CurrentState.OnAttackInput();
        controls.PlayerMovement.Dodge.performed += ctx => stateMachine.CurrentState.OnDashInput();
        controls.PlayerMovement.LockOn.performed += ctx => ToggleLockOn();


        // 런타임에 애니메이션 클립을 동적으로 갈아끼우기 위한 오버라이드 컨트롤러 세팅
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
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

        stateMachine.Update();
    }

    private void LateUpdate()
    {
        // 카메라 중심축(cameraTarget)이 할당되지 않았다면 에러 방지
        if (cameraTransform == null || cameraTarget == null) return;

        // [카메라 핵심 1] 중심축이 플레이어의 '위치'만 따라갑니다. 
        // 플레이어의 '회전'은 무시하므로, 캐릭터가 구르거나 빙글빙글 돌아도 카메라는 요동치지 않습니다.
        // 캐릭터 발밑이 아니라 가슴이나 머리 높이를 중심점으로 삼도록 Y값을 1.5f 정도 올려줍니다.
        cameraTarget.position = transform.position + Vector3.up * 1.5f;

        // 락온 카메라 처리
        if (isLockedOn && currentTarget != null)
        {
            Vector3 dirToTarget = currentTarget.position - cameraTarget.position;
            float distanceToTarget = dirToTarget.magnitude;

            // 1. 수직(Y축) 흔들림 억제 (기존 유지)
            if (distanceToTarget < 10.0f)
            {
                dirToTarget.y *= (distanceToTarget / 10.0f);
            }

            Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
            float angleToTarget = Vector3.Angle(cameraTarget.forward, dirToTarget);

            // 2. 거리 비례 기본 감속 (초근접 시 휙휙 도는 것 방지)
            float currentRotationSpeed = 15f;
            if (distanceToTarget < 1.0f)
            {
                currentRotationSpeed = Mathf.Lerp(2f, 15f, distanceToTarget / 2.0f);
            }

            // 3. [핵심 수정] 소프트 데드존 (Soft Deadzone)
            // 하드하게 멈추지 않고, 10도 이내로 들어오면 각도에 비례해서 속도를 부드럽게 줄입니다.
            if (angleToTarget < 10.0f)
            {
                // 각도가 5도면 속도 100%, 2.5도면 50%, 0도면 0%로 스무스하게 브레이크가 걸립니다.
                currentRotationSpeed *= (angleToTarget / 5.0f);
            }

            //  if문으로 회전을 막지 않고, 매 프레임 Slerp를 실행하여 부드러움을 유지합니다.
            cameraTarget.rotation = Quaternion.Slerp(cameraTarget.rotation, targetRot, currentRotationSpeed * Time.deltaTime);

            // 락온이 풀렸을 때 마우스 튀는 현상 방지
            Vector3 euler = cameraTarget.rotation.eulerAngles;
            cameraYaw = euler.y;
            cameraPitch = euler.x > 180 ? euler.x - 360 : euler.x;
        }
        else
        {
            // 자유 카메라 처리 (기존 코드 유지)
            float lookX = mouseMoveInput.x * mouseSensitivity * 0.1f;
            float lookY = mouseMoveInput.y * mouseSensitivity * 0.1f;

            cameraYaw += lookX;
            cameraPitch -= lookY;
            cameraPitch = Mathf.Clamp(cameraPitch, bottomClamp, topClamp);

            cameraTarget.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
        }
    }

    public void EquipWeapon(Attack_SO_Data newData)
    {
        currentWeapon = newData;

        // 무기 데이터에 있는 클립들을 애니메이터 노드에 매핑
        // (예: 빈 슬롯 "Attack1"에 실제 클립 "대검_1타"를 끼워 넣음)
        for (int i = 0; i < newData.comboAttacks.Length; i++)
        {
            overrideController[$"Attack{i + 1}"] = newData.comboAttacks[i].clip;
        }

        if (currentDodgeData != null)
        {
            overrideController["Roll"] = currentDodgeData.clip;
        }
    }

    // [루트 모션 핵심] 애니메이션이 스스로 이동하는 물리량(deltaPosition)을 가로채서 제어하는 함수
    private void OnAnimatorMove()
    {
        if (animator.applyRootMotion && characterController != null)
        {
            // 1. 락온 중 구르기 상태일 때
            if (stateMachine.CurrentState == dashState && currentRollDir != Vector3.zero)
            {
                Vector3 delta = animator.deltaPosition;
                float moveDistance = new Vector3(delta.x, 0f, delta.z).magnitude;

                Vector3 finalMove = currentRollDir * moveDistance;
                // finalMove.y = delta.y; <-- 기존의 수직 이동 보존 코드 삭제 (이제 중력이 전담함)

                MoveCharacter(finalMove); // [수정] 통합 함수 호출
            }
            else
            {
                // 2. 평소(공격, 걷기 등)
                Vector3 rootMotion = animator.deltaPosition;
                rootMotion.y = 0f; // [수정] 애니메이션 자체의 Y축 덜덜거림 무시

                MoveCharacter(rootMotion); // [수정] 통합 함수 호출
            }
        }
    }

    private void ToggleLockOn() // 락온 켜기/끄기
    {
        // 이미 락온 중이면 해제
        if (isLockedOn)
        {
            isLockedOn = false;
            currentTarget = null;
            animator.SetBool("isLockedOn", false);
            return;
        }

        
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockOnRadius, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (Collider col in colliders)
        {
            // 1. 적이 내 카메라 시야(화면의 앞쪽)에 있는지 내적(Dot)으로 검사
            // (주의: cameraTransform 대신 회전 중심축인 cameraTarget을 기준으로 계산해야 정확합니다)
            Vector3 dirToTarget = (col.transform.position - cameraTarget.position).normalized;
            float dot = Vector3.Dot(cameraTarget.forward, dirToTarget);

            if (dot > 0.5f) // 약 60도 시야각 이내에 있는 적만 판별 (1.0이 정면, 0이 측면, -1이 후면)
            {
                // 2. 시야 내에 있는 적들 중 '가장 가까운 적' 찾기
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestTarget = col.transform;
                }
            }
        }

        // 적을 찾았다면 락온 상태로 전환
        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            isLockedOn = true;
        }
        else
        {
            isLockedOn = false;
            currentTarget = null;
        }

        // 애니메이터에 락온 상태를 전달하여 8방향 게걸음 Blend Tree가 부드럽게 전환되게 만듭니다.
        animator.SetBool("isLockedOn", isLockedOn);
    }

    //중력 적용 후 움직이기
    public void MoveCharacter(Vector3 horizontalMotion)
    {
        // 1. 중력 계산
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0.0f) verticalVelocity = -2f; // 땅에 딱 붙어있도록 작은 음수 유지
        }
        else
        {
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime; // 공중이면 낙하
        }

        // 2. 수평 이동(걷기/루트모션) + 수직 이동(중력) 합치기
        horizontalMotion.y += verticalVelocity * Time.deltaTime;

        // 3. 최종 이동 실행 (모든 프레임에서 딱 한 번만 호출됨!)
        characterController.Move(horizontalMotion);
    }

    public void TakeDamage(float damage)
    {
        //currentHealth -= damage;

        if (/*currentHealth <= 0*/ false)
        {
            // 체력이 0 이하면 죽음 상태로 (구현 예정)
            // stateMachine.ChangeState(deadState);
        }
        else
        {
            stateMachine.ChangeState(hitState);
        }
    }
    private void OnEnable() { controls.Enable(); }
    private void OnDisable() { controls.Disable(); }
}