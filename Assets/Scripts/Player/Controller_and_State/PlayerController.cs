using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public Transform cameraTransform;    // 상하 회전을 시킬 카메라(또는 머리) 오브젝트
    private float cameraPitch = 0f; // 상하 회전 각도 (X축)
    private float cameraYaw = 0f;   // 좌우 회전 각도 (Y축)
    public float topClamp = 70f;    // 위로 쳐다볼 수 있는 최대 각도
    public float bottomClamp = -30f;// 아래로 내려다볼 수 있는 최대 각도


    [Header("Components")]
    public Animator animator;
    //무기 교체
    public weapon_1_ob currentWeapon;

    private AnimatorOverrideController overrideController;

    [HideInInspector] public bool canCombo;      // 현재 클릭 시 예약 가능 여부
    [HideInInspector] public bool canNextAttack; // 실제 다음 애니메이션 전환 가능 시점
    [HideInInspector] public bool canCancel;     // 이동/회피로 캔슬 가능 여부

    

    public CharacterController characterController;
    // 핵심: 분리된 상태 머신 객체
    public StateMachine stateMachine { get; private set; }

    // 생성해둔 상태들 (캐싱)
    public IdleState idleState { get; private set; }
    public MoveState moveState { get; private set; }
    public AttackState attackState { get; private set; }
    public DodgeState dashState { get; private set; }

    private void Awake()
    {
        controls = new InputSystem_Actions();
        stateMachine = new StateMachine();

        // 1. 상태 인스턴스화 (자신과 상태머신을 넘겨줌)
        idleState = new IdleState(this, stateMachine);
        moveState = new MoveState(this, stateMachine);
        attackState = new AttackState(this, stateMachine);
        dashState = new DodgeState(this, stateMachine);

        // 2. New Input System 이벤트 연결
        // Controller가 직접 상태를 바꾸지 않고, 현재 상태에게 "공격버튼 눌렸어!"라고 전달만 합니다.
        controls.PlayerMovement.Attack.performed += ctx => stateMachine.CurrentState.OnAttackInput();

        // (만약 Dash 액션을 만드셨다면 아래처럼 연결합니다)
        controls.PlayerMovement.Dodge.performed += ctx => stateMachine.CurrentState.OnDashInput();

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (currentWeapon != null) EquipWeapon(currentWeapon);
        // 게임 시작 시 초기 상태 지정
        stateMachine.Initialize(idleState);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // 매 프레임 입력값 읽기
        moveInput = controls.PlayerMovement.Move.ReadValue<Vector2>();
        mouseMoveInput = controls.PlayerMovement.Look.ReadValue<Vector2>();


        // 상태 머신 실행
        stateMachine.Update();
    }
    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 1. 입력값 확인 (Input System에서 Delta 값을 제대로 받아오는지 확인)
        float lookX = mouseMoveInput.x * mouseSensitivity * 0.1f; // deltaTime 대신 0.1f 등으로 고정해서 테스트해보세요
        float lookY = mouseMoveInput.y * mouseSensitivity * 0.1f;

        // 2. 값 누적
        cameraYaw += lookX;
        cameraPitch -= lookY;

        // 3. 각도 제한
        cameraPitch = Mathf.Clamp(cameraPitch, bottomClamp, topClamp);

        // 4. 실제 적용 (이 부분이 실행되는지 Debug.Log로 확인 필요)
        cameraTransform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);

        // Debug.Log($"Yaw: {cameraYaw}, Pitch: {cameraPitch}"); // 값이 변하는지 콘솔창에서 확인하세요.
    }
    public void EquipWeapon(weapon_1_ob newData)
    {
        currentWeapon = newData;

        // 무기 데이터에 있는 클립들을 애니메이터 노드에 매핑
        // "Attack1"이라는 노드 이름을 실제 파일(newData.comboClips[0])로 교체
        for (int i = 0; i < newData.comboClips.Length; i++)
        {
            overrideController[$"Attack{i + 1}"] = newData.comboClips[i];
        }
    }

    private void OnAnimatorMove()
    {
        if (animator.applyRootMotion && characterController != null)
        {
            // 애니메이션의 프레임당 이동량(deltaPosition)을 가져와서 CharacterController.Move에 적용 (벽 뚫기 방지)
            characterController.Move(animator.deltaPosition);
        }
    }

    private void OnEnable() { controls.Enable(); }
    private void OnDisable() { controls.Disable(); }
}
