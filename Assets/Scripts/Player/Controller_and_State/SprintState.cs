using UnityEngine;

public class SprintState : PlayerBaseState
{
    private readonly int sprintAnimHash = Animator.StringToHash("Sprint");

    public SprintState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("상태: Sprint");
        player.animator.CrossFadeInFixedTime(sprintAnimHash, 0.1f);
    }

    public override void Update()
    {
        if (player.moveInput == Vector2.zero || !player.isSprintButtonHeld)
        {
            stateMachine.ChangeState(player.moveState);
            return;
        }

        HandleRotation();

        Vector3 camForward = player.cameraTransform.forward;
        Vector3 camRight = player.cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camRight * player.moveInput.x + camForward * player.moveInput.y;
        player.characterController.Move(moveDir * player.moveSpeed * 1.5f * Time.deltaTime);
    }

    public override void OnAttackInput() { stateMachine.ChangeState(player.attackState); }

    public override void Exit()
    {
        // 락온이 아닐 때만 달리기 방향으로 루트를 동기화하여 덜덜거림 방지
        if (!player.isLockedOn)
        {
            player.transform.rotation = player.model.transform.rotation;
            player.model.transform.localRotation = Quaternion.identity;
        }
    }

    // 전력질주 전용 회전 (Root 고정, Model만 회전)
    protected override void HandleRotation()
    {
        if (player.moveInput != Vector2.zero)
        {
            Vector3 camForward = player.cameraTransform.forward;
            Vector3 camRight = player.cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camRight * player.moveInput.x + camForward * player.moveInput.y).normalized;

            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                player.model.transform.rotation = Quaternion.Slerp(player.model.transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
    }
}
