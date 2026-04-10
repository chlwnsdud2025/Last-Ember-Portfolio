using UnityEngine;

public class MoveState : PlayerBaseState
{
    private readonly int runAnimHash = Animator.StringToHash("Run");
    public MoveState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter() {
        Debug.Log("상태: Move");

        player.animator.CrossFadeInFixedTime(runAnimHash, 0.1f);

    }

    public override void Update()
    {
        if (player.moveInput == Vector2.zero)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }
        HandleRotation();
        Vector3 camForward = player.cameraTransform.forward;
        Vector3 camRight = player.cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move1 = camRight * player.moveInput.x + camForward * player.moveInput.y;

        player.characterController.Move(move1 * player.moveSpeed * Time.deltaTime);
    }


}

