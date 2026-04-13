using UnityEngine;

public class MoveState : PlayerBaseState
{
    // 애니메이션 해시를 미리 선언하여 성능을 최적화하고 값 변조를 방지합니다.
    private readonly int runAnimHash = Animator.StringToHash("Run");
    private readonly int lockOnRunAnimHash = Animator.StringToHash("LockOnRun");

    public MoveState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("상태: Move");

        // 진입 시점에 현재 락온 상태에 맞는 애니메이션을 재생합니다.
        int initialAnim = player.isLockedOn ? lockOnRunAnimHash : runAnimHash;
        player.animator.CrossFadeInFixedTime(initialAnim, 0.1f);
    }

    public override void Update()
    {
        // 1. 이동 입력이 없으면 Idle로 전환
        if (player.moveInput == Vector2.zero)
        {
            // 발 미끄러짐 방지를 위해 파라미터 값을 0으로 부드럽게 초기화합니다.
            player.animator.SetFloat("InputX", 0f, 0.1f, Time.deltaTime);
            player.animator.SetFloat("InputY", 0f, 0.1f, Time.deltaTime);
            stateMachine.ChangeState(player.idleState);
            return;
        }

        // 2. 전력질주(Sprint) 및 걷기(Walk) 전환
        if (player.isSprintButtonHeld)
        {
            stateMachine.ChangeState(player.sprintState);
            return;
        }

        if (player.isWalkButtonHeld)
        {
            stateMachine.ChangeState(player.walkState);
            return;
        }

        // 3. [핵심] 락온 여부에 따른 애니메이션 실시간 전환 로직 (CrossFade)
        // 현재 애니메이터의 상태 정보를 가져와 애니메이션을 강제 교체합니다.
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        if (player.isLockedOn)
        {
            // 락온 중인데 일반 달리기 중이라면, 8방향 게걸음 블렌드 트리로 전환합니다.
            if (stateInfo.shortNameHash == runAnimHash && !player.animator.IsInTransition(0))
            {
                player.animator.CrossFadeInFixedTime(lockOnRunAnimHash, 0.1f);
            }

            // 게걸음 블렌딩을 위해 X, Y 입력값을 파라미터로 전달합니다.
            player.animator.SetFloat("InputX", player.moveInput.x, 0.1f, Time.deltaTime);
            player.animator.SetFloat("InputY", player.moveInput.y, 0.1f, Time.deltaTime);
        }
        else
        {
            // 비 락온 상태인데 락온용 애니메이션이 재생 중이라면, 일반 달리기로 복귀합니다.
            if (stateInfo.shortNameHash == lockOnRunAnimHash && !player.animator.IsInTransition(0))
            {
                player.animator.CrossFadeInFixedTime(runAnimHash, 0.1f);
            }
        }

        // 4. 이동 및 회전 처리 (기존 구조 유지)
        HandleRotation();

        Vector3 camForward = player.cameraTransform.forward;
        Vector3 camRight = player.cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camRight * player.moveInput.x + camForward * player.moveInput.y;
        player.characterController.Move(moveDir * player.moveSpeed * Time.deltaTime);
    }
}