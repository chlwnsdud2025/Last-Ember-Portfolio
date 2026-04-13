using UnityEngine;

public class DodgeState : PlayerBaseState
{
    private readonly int dodgeAnimHash = Animator.StringToHash("Roll");
    private bool attackBuffered = false;
    private bool dodgeBuffered = false;

    public DodgeState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("상태: Roll");
        player.isInvincible = false;
        attackBuffered = false;
        dodgeBuffered = false;

        // 구를 방향 설정 및 껍데기(Model) 회전
        if (player.moveInput != Vector2.zero)
        {
            Vector3 camForward = player.cameraTransform.forward;
            Vector3 camRight = player.cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            player.currentRollDir = (camRight * player.moveInput.x + camForward * player.moveInput.y).normalized;
        }
        else
        {
            player.currentRollDir = -player.transform.forward; // 입력 없을 시 백스텝
        }

        if (player.currentRollDir != Vector3.zero)
        {
            player.model.transform.rotation = Quaternion.LookRotation(player.currentRollDir);
        }

        player.animator.applyRootMotion = true;
        player.animator.CrossFadeInFixedTime(dodgeAnimHash, 0.1f, 0, 0f);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        float progress = stateInfo.normalizedTime;
        Roll_SO_Data dodgeData = player.currentDodgeData;

        if (stateInfo.shortNameHash == dodgeAnimHash && !player.animator.IsInTransition(0))
        {
            player.isInvincible = (progress >= dodgeData.iframeStart && progress <= dodgeData.iframeEnd);

            if (progress >= dodgeData.cancelWindow)
            {
                if (attackBuffered) { stateMachine.ChangeState(player.attackState); return; }
                else if (dodgeBuffered) { stateMachine.ChangeState(player.dashState); return; }
                else if (player.moveInput != Vector2.zero) { stateMachine.ChangeState(player.moveState); return; }
            }

            if (progress >= dodgeData.animationEnd)
            {
                stateMachine.ChangeState(player.idleState);
            }
        }
    }

    public override void Exit()
    {
        player.isInvincible = false;
        player.animator.applyRootMotion = false;
        player.currentRollDir = Vector3.zero;

        if (!player.isLockedOn)
        {
            player.transform.rotation = player.model.transform.rotation;
            player.model.transform.localRotation = Quaternion.identity;
        }
    }

    public override void OnAttackInput()
    {
        if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= player.currentDodgeData.preInputWindowStart)
            attackBuffered = true;
    }

    public override void OnDashInput()
    {
        if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= player.currentDodgeData.preInputWindowStart)
            dodgeBuffered = true;
    }
}
