using UnityEngine;

public class MoveState : PlayerBaseState
{
    private readonly int runAnimHash = Animator.StringToHash("Run");
    private readonly int lockOnRunAnimHash = Animator.StringToHash("LockOnRun");

    public MoveState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        //Debug.Log("상태: Move");

        int initialAnim = player.isLockedOn ? lockOnRunAnimHash : runAnimHash;
        player.animator.CrossFadeInFixedTime(initialAnim, 0.1f);
    }

    public override void Update()
    {
        // Idle 전환
        if (player.moveInput == Vector2.zero)
        {
            player.animator.SetFloat("InputX", 0f, 0.1f, Time.deltaTime);
            player.animator.SetFloat("InputY", 0f, 0.1f, Time.deltaTime);
            stateMachine.ChangeState(player.idleState);
            return;
        }

        // Sprint 및 Walk 전환
        if (player.isSprintButtonHeld && player.stats.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.sprintState);
            return;
        }

        if (player.isWalkButtonHeld)
        {
            stateMachine.ChangeState(player.walkState);
            return;
        }

        
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

        // 이동 및 회전 처리 (기존 구조 유지)
        HandleRotation();

        Vector3 camForward = player.cameraTransform.forward;
        Vector3 camRight = player.cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camRight * player.moveInput.x + camForward * player.moveInput.y;
        player.MoveCharacter(moveDir * player.moveSpeed * Time.deltaTime);
        //player.characterController.Move(moveDir * player.moveSpeed * Time.deltaTime);
    }
}