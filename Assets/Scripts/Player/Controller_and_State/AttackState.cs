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
            Vector3 attackDir = Vector3.zero;

            // 락온 상태일 시 (방향키 무시하고 무조건 적을 향함)
            if (player.isLockedOn && player.currentTarget != null)
            {
                attackDir = (player.currentTarget.position - player.transform.position).normalized;
                attackDir.y = 0f; // 하늘이나 땅을 보지 않게 방지
            }
            // 락온 상태가 아닐 시 (기존처럼 입력한 방향을 향함)
            else if (player.moveInput != Vector2.zero)
            {
                Vector3 camForward = player.cameraTransform.forward;
                Vector3 camRight = player.cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                attackDir = (camRight * player.moveInput.x + camForward * player.moveInput.y).normalized;
            }

            // 최종적으로 결정된 방향(attackDir)으로 몸을 부드럽게 돌립니다.
            if (attackDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(attackDir);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, 15f * Time.deltaTime);
                player.model.transform.localRotation = Quaternion.Slerp(player.model.transform.localRotation, Quaternion.identity, 15f * Time.deltaTime);
            }
        }

        //무기의 colider 켰다 끄기
        if (isPlayingAttack && !player.animator.IsInTransition(0))
        {
            // 현재 진행도가 판정 시작점과 종료점 사이라면?
            bool shouldHitboxBeActive = (progress >= currentAttackData.hitboxStartTime && progress <= currentAttackData.hitboxEndTime);

            if (shouldHitboxBeActive)
            {
                // 콜라이더 켜기 + 데미지 계산해서 넘겨주기
                player.currentWeaponScript.currentDamage = player.currentWeapon.baseDamage * currentAttackData.damageMultiplier;
                player.currentWeaponScript.EnableHitbox();
            }
            else
            {
                // 범위 밖이면 콜라이더 끄기
                player.currentWeaponScript.DisableHitbox();
            }
        }
        else
        {
            // 트랜지션 중이거나 찌꺼기 프레임일 때는 무조건 끕니다 (안전 장치)
            player.currentWeaponScript.DisableHitbox();
        }

        // 콤보 전환 로직
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

        // 이동/회피 캔슬 로직
        if (player.canCancel && player.moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(player.moveState);
            return;
        }

        // 자연 종료, 지금 재생 중인 모션이 '진짜 공격 애니메이션'일 때만 종료되도록 수정
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
            player.currentWeaponScript.DisableHitbox();

            comboIndex = 0;
            player.animator.applyRootMotion = false;

        }
    }

    public override void OnDashInput() { /* 무시 */ }
}