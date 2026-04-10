using UnityEngine;

public class AttackState : PlayerBaseState
{
    private int comboIndex = 0;
    private bool comboInputReceived = false;
    private bool isContinuingCombo = false;

    // 회전을 허용할 애니메이션 진행도 (0.0 ~ 1.0)
    // 0.3f면 애니메이션의 30%가 재생될 때까지만 방향을 틀 수 있습니다.
    private float trackingWindow = 0.3f;
    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        isContinuingCombo = false;
        comboInputReceived = false;

        player.animator.applyRootMotion = true;

        string stateName = $"Attack{comboIndex + 1}";
        player.animator.CrossFadeInFixedTime(stateName, 0.1f);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // 1. 공격 초반 방향 보정 (Attack Tracking)
        // 트랜지션 중이거나 애니메이션 진행도가 설정값(trackingWindow) 이하일 때만 부드럽게 회전
        if (player.animator.IsInTransition(0) || stateInfo.normalizedTime <= trackingWindow)
        {
            if (player.moveInput != Vector2.zero)
            {
                Vector3 camForward = player.cameraTransform.forward;
                Vector3 camRight = player.cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 attackDir = (camRight * player.moveInput.x + camForward * player.moveInput.y).normalized;

                if (attackDir != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(attackDir);
                    // 즉시 회전하지 않고 Slerp를 사용해 부드럽게 따라가도록 수정 (15f는 회전 속도)
                    player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, 15f * Time.deltaTime);
                    player.model.transform.localRotation = Quaternion.identity;
                }
            }
        }

        // 2. 콤보 전환 로직
        if (comboInputReceived && player.canNextAttack)
        {
            if (comboIndex + 1 < player.currentWeapon.comboClips.Length)
            {
                comboIndex++;
                isContinuingCombo = true;
                stateMachine.ChangeState(this);
                return;
            }
        }

        // 3. 이동/회피 캔슬 로직
        if (player.canCancel)
        {
            if (player.moveInput != Vector2.zero)
            {
                stateMachine.ChangeState(player.moveState);
                return;
            }
        }

        // 4. 자연 종료
        if (!player.animator.IsInTransition(0) && stateInfo.normalizedTime >= 0.95f)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void OnAttackInput()
    {
        if (player.canCombo)
        {
            comboInputReceived = true;
            Debug.Log($"{comboIndex + 1}타 중 다음 콤보 예약됨!");
        }
    }

    public override void Exit()
    {
        if (!isContinuingCombo)
        {
            comboIndex = 0;
            player.animator.applyRootMotion = false;
        }
    }

    public override void OnDashInput() { /* 무시 */ }
}
