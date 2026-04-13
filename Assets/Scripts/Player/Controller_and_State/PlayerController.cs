using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using static Attack_SO_Data;

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

    // [중요] 카메라 아키텍처 분리
    // cameraTarget: 플레이어의 '위치'만 따라다니며 회전의 중심축 역할을 하는 빈 오브젝트
    // cameraTransform: 실제 화면을 비추는 Main Camera (cameraTarget의 하위에 배치됨)
    public Transform cameraTarget;
    public Transform cameraTransform;

    private float cameraPitch = 0f; // 상하 회전 각도 (X축)
    private float cameraYaw = 0f;   // 좌우 회전 각도 (Y축)
    public float topClamp = 70f;    // 위로 쳐다볼 수 있는 최대 각도
    public float bottomClamp = -30f;// 아래로 내려다볼 수 있는 최대 각도

    [Header("Lock-On Settings")]
    public float lockOnRadius = 15f;     // 적을 찾는 반경
    public LayerMask enemyLayer;         // 적을 판별할 레이어
    [HideInInspector] public Transform currentTarget;
    [HideInInspector] public bool isLockedOn = false;

    [Header("Components")]
    public Animator animator;
    public CharacterController characterController;
    private AnimatorOverrideController overrideController;

    [Header("Combat & Dodge")]
    public Attack_SO_Data currentWeapon;
    public Roll_SO_Data currentDodgeData;

    [HideInInspector] public bool canCombo;      // 현재 클릭 시 연속 공격 예약 가능 여부
    [HideInInspector] public bool canNextAttack; // 실제 다음 애니메이션으로 전환 가능한 시점
    [HideInInspector] public bool canCancel;     // 이동/회피로 현재 모션을 캔슬할 수 있는 여부
    [HideInInspector] public bool isInvincible = false; // 현재 구르기 무적 상태인지 확인하는 플래그

    // [중요] 루트 모션(애니메이션 이동)의 방향을 코드로 강제 오버라이드 하기 위한 변수
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

    private void Awake()
    {
        controls = new InputSystem_Actions();
        stateMachine = new StateMachine();

        // 상태 인스턴스화 (자신과 상태머신을 넘겨줌)
        idleState = new IdleState(this, stateMachine);
        moveState = new MoveState(this, stateMachine);
        attackState = new AttackState(this, stateMachine);
        dashState = new DodgeState(this, stateMachine);
        sprintState = new SprintState(this, stateMachine);
        walkState = new WalkState(this, stateMachine);

        // 단발성 액션 이벤트 연결 (버튼을 누른 순간 1회만 실행됨)
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
        // 1. 매 프레임 스틱/마우스 입력 값 읽기
        moveInput = controls.PlayerMovement.Move.ReadValue<Vector2>();
        mouseMoveInput = controls.PlayerMovement.Look.ReadValue<Vector2>();

        // 2. 지속형 액션(Hold) 상태 읽기 
        // -> 이벤트가 씹히는 버그를 막기 위해 매 프레임 직접 눌려있는지(IsPressed) 검사합니다.
        isSprintButtonHeld = controls.PlayerMovement.Sprint.IsPressed();
        isWalkButtonHeld = controls.PlayerMovement.Walk.IsPressed();

        // 3. 현재 활성화된 State의 Update 로직 실행
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

        // [카메라 핵심 2] 락온 카메라 처리
        if (isLockedOn && currentTarget != null)
        {
            // 실제 카메라(cameraTransform)가 아니라 뼈대(cameraTarget)가 적을 바라보게 회전시킵니다.
            Vector3 dirToTarget = currentTarget.position - cameraTarget.position;
            Quaternion targetRot = Quaternion.LookRotation(dirToTarget);

            cameraTarget.rotation = Quaternion.Slerp(cameraTarget.rotation, targetRot, 15f * Time.deltaTime);

            // 락온이 풀렸을 때 마우스 회전값이 확 튀지 않도록 내부 변수(Yaw, Pitch)를 현재 각도에 강제로 맞춰둡니다.
            Vector3 euler = cameraTarget.rotation.eulerAngles;
            cameraYaw = euler.y;
            cameraPitch = euler.x > 180 ? euler.x - 360 : euler.x; // 유니티의 360도 체계를 음수/양수 체계로 변환
        }
        else // [카메라 핵심 3] 자유 카메라 처리 (락온을 안 했을 때)
        {
            float lookX = mouseMoveInput.x * mouseSensitivity * 0.1f;
            float lookY = mouseMoveInput.y * mouseSensitivity * 0.1f;

            cameraYaw += lookX;
            cameraPitch -= lookY;
            cameraPitch = Mathf.Clamp(cameraPitch, bottomClamp, topClamp);

            // 마우스 움직임에 따라 뼈대(cameraTarget)를 회전시킵니다.
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
            // 1. 락온 중 구르기 상태일 때는 루트 모션 '방향'을 코드로 강제 보정합니다!
            // 이유: 락온 중에는 캐릭터의 뿌리(Root)가 적을 쳐다보게 고정되므로,
            // 그대로 구르면 '옆구르기' 애니메이션이 나오면서 물리적으로는 '앞(적 방향)'으로 전진해버리는 모순이 생깁니다.
            if (stateMachine.CurrentState == dashState && currentRollDir != Vector3.zero)
            {
                Vector3 delta = animator.deltaPosition;

                // 애니메이션이 원래 이동하려던 '수평 이동 거리(magnitude)'만 훔쳐옵니다.
                float moveDistance = new Vector3(delta.x, 0f, delta.z).magnitude;

                // 유저가 조작한 방향(currentRollDir)에 훔쳐온 거리(moveDistance)를 곱해서 새로운 이동 벡터를 만듭니다.
                Vector3 finalMove = currentRollDir * moveDistance;
                finalMove.y = delta.y; // 점프나 중력 등 수직 이동값은 원래 애니메이션 값을 보존

                characterController.Move(finalMove);
            }
            else
            {
                // 2. 평소(공격, 걷기 등)에는 꼬일 일이 없으므로 기존 애니메이션 루트 모션을 그대로 사용합니다.
                characterController.Move(animator.deltaPosition);
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
            animator.SetBool("isLockedOn", false); // [추가] 애니메이터에도 해제되었음을 알림
            return;
        }

        // 주변의 적 탐색 (내 위치를 중심으로 lockOnRadius 반경 내의 투망을 던짐)
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

    private void OnEnable() { controls.Enable(); }
    private void OnDisable() { controls.Disable(); }
}