using UnityEngine;

public class DodgeState : PlayerBaseState
{
    private readonly int dodgeAnimHash = Animator.StringToHash("Roll");

    public DodgeState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("상태: Roll");

        if (player.moveInput != Vector2.zero)
        {
            Vector3 camForward = player.cameraTransform.forward;
            Vector3 camRight = player.cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 rollDir = (camRight * player.moveInput.x + camForward * player.moveInput.y).normalized;

            if (rollDir != Vector3.zero)
            {
                // [핵심 수정] model이 아니라 루트(Player) 자체를 굴러갈 방향으로 즉시 회전시킵니다.
                player.transform.rotation = Quaternion.LookRotation(rollDir);

                // 혹시 모델의 로컬 회전이 꼬여있을 수 있으니 정렬해줍니다.
                player.model.transform.localRotation = Quaternion.identity;
            }
        }

        player.animator.applyRootMotion = true;
        player.animator.CrossFadeInFixedTime(dodgeAnimHash, 0.1f);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.shortNameHash == dodgeAnimHash && !player.animator.IsInTransition(0))
        {
            if (stateInfo.normalizedTime >= 0.75f)
            {
                if (player.moveInput != Vector2.zero)
                    stateMachine.ChangeState(player.moveState);
                else
                    stateMachine.ChangeState(player.idleState);
            }
        }
    }

    public override void Exit()
    {
        player.animator.applyRootMotion = false;
    }

    public override void OnAttackInput() { /* 무시 */ }
    public override void OnDashInput() { /* 무시 */ }
}
