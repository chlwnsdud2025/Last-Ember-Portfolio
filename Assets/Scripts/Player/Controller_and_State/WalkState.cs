using UnityEngine;

public class WalkState : PlayerBaseState
{
    // 애니메이터에 있는 걷기 노드 이름 (예: "Walk" 또는 "Movement" 블렌드트리)
    private readonly int walkAnimHash = Animator.StringToHash("Walk");

    public WalkState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        //Debug.Log("상태: Walk");

        // 락온 상태일 때는 8방향 walk 노드로 진입, 아니면 일반 걷기 노드로 진입
        if (player.isLockedOn)
        {
            player.animator.CrossFadeInFixedTime(Animator.StringToHash("LockOnWalk"), 0.1f);
        }
        else
        {
            player.animator.CrossFadeInFixedTime(walkAnimHash, 0.1f);
        }
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

        // Sprint 전환
        if (player.isSprintButtonHeld)
        {
            stateMachine.ChangeState(player.sprintState);
            return;
        }

        // move 전환
        if (!player.isWalkButtonHeld)
        {
            stateMachine.ChangeState(player.moveState);
            return;
        }

        // 애니메이터 파라미터 전달
        if (player.isLockedOn)
        {
            player.animator.SetFloat("InputX", player.moveInput.x, 0.1f, Time.deltaTime);
            player.animator.SetFloat("InputY", player.moveInput.y, 0.1f, Time.deltaTime);
        }
        

        // 실제 이동 및 회전 처리
        HandleRotation();
        Vector3 camForward = player.cameraTransform.forward;
        Vector3 camRight = player.cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camRight * player.moveInput.x + camForward * player.moveInput.y;
        player.MoveCharacter(moveDir * (player.moveSpeed * 0.5f) * Time.deltaTime);
        //player.characterController.Move(moveDir * (player.moveSpeed * 0.5f) * Time.deltaTime);
    }
}
