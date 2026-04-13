using UnityEngine;
using static Attack_SO_Data;

public class AttackState : PlayerBaseState
{
    private int comboIndex = 0;
    private bool comboInputReceived = false;
    private bool isContinuingCombo = false;
    private float trackingWindow = 0.3f;

    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        isContinuingCombo = false;
        comboInputReceived = false;
        player.canCombo = false;
        player.canNextAttack = false;
        player.canCancel = false;

        player.animator.applyRootMotion = true;

        string stateName = $"Attack{comboIndex + 1}";
        player.animator.CrossFadeInFixedTime(stateName, 0.1f, 0, 0f);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        float progress = stateInfo.normalizedTime;

        AttackData currentAttackData = player.currentWeapon.comboAttacks[comboIndex];

        //  현재 애니메이터가 실제로 '이번 타수의 공격 애니메이션'을 재생 중인지 확인합니다.
        int expectedHash = Animator.StringToHash($"Attack{comboIndex + 1}");
        bool isPlayingAttack = (stateInfo.shortNameHash == expectedHash);

        if (isPlayingAttack && !player.animator.IsInTransition(0))
        {
            player.canCombo = (progress >= currentAttackData.comboWindowStart && progress <= currentAttackData.comboWindowEnd);
            player.canNextAttack = (progress >= currentAttackData.comboTransitionPoint);
            player.canCancel = (progress >= currentAttackData.cancelWindowStart);
        }
        else
        {
            player.canCombo = false;
            player.canNextAttack = false;
            player.canCancel = false;
        }

        //공격 초반 방향 보정
        if (player.animator.IsInTransition(0) || (isPlayingAttack && progress <= trackingWindow))
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

                    //  카메라가 분리되었으므로 껍데기(Model)가 아니라 루트(Player) 자체를 회전시킵니다!
                    // 이렇게 해야 애니메이션의 루트 모션이 내가 입력한 방향으로 정확하게 돌진합니다.
                    player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, 15f * Time.deltaTime);

                    // 혹시 어긋나 있을지 모르는 껍데기의 로컬 회전값을 정면으로 부드럽게 맞춰줍니다.
                    player.model.transform.localRotation = Quaternion.Slerp(player.model.transform.localRotation, Quaternion.identity, 15f * Time.deltaTime);
                }
            }
        }

        // 2. 콤보 전환 로직
        if (comboInputReceived && player.canNextAttack)
        {
            if (comboIndex + 1 < player.currentWeapon.comboAttacks.Length)
            {
                comboIndex++;
                isContinuingCombo = true;
                stateMachine.ChangeState(this);
                return;
            }
        }

        // 3. 이동/회피 캔슬 로직
        if (player.canCancel && player.moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(player.moveState);
            return;
        }

        // 4. 자연 종료, 지금 재생 중인 모션이 '진짜 공격 애니메이션'일 때만 종료되도록 수정
        if (isPlayingAttack && !player.animator.IsInTransition(0) && progress >= 0.95f)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void OnAttackInput()
    {
        if (player.canCombo)
        {
            comboInputReceived = true;
        }
    }

    public override void Exit()
    {
        if (!isContinuingCombo)
        {
            comboIndex = 0;
            player.animator.applyRootMotion = false;

            // 루트를 직접 회전시켰으므로, Exit에서 강제로 껍데기를 맞춰줄 필요가 사라져 코드가 깔끔해졌습니다.
        }
    }

    public override void OnDashInput() { /* 무시 */ }
}