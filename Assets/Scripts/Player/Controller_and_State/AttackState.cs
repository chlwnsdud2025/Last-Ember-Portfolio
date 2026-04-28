using UnityEngine;
using static Attack_SO_Data;

public class AttackState : PlayerBaseState
{
    private int comboIndex = 0;
    private bool comboInputReceived = false;
    private bool isContinuingCombo = false;
    private float trackingWindow = 0.3f;
    private bool dodgeInputReceived = false;
    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        isContinuingCombo = false;
        comboInputReceived = false;
        player.canCombo = false;
        player.canNextAttack = false;
        player.canCancel = false;
        dodgeInputReceived = false;
        player.animator.applyRootMotion = true;

        AttackData currentAttackData = player.currentWeapon.comboAttacks[comboIndex];
        player.stats.UseStamina(currentAttackData.steminareduce);

        string stateName = $"Attack{comboIndex + 1}";
        player.animator.CrossFadeInFixedTime(stateName, 0.1f, 0, 0f);
    }

    public override void Update()
    {
        player.stats.DelayRegen();

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

            // 락온 상태일 시 (무조건 적 방향)
            if (player.isLockedOn && player.currentTarget != null)
            {
                attackDir = (player.currentTarget.position - player.transform.position).normalized;
                attackDir.y = 0f; 
            }
            // 락온 상태가 아닐 시 (입력한 방향)
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
            bool shouldHitboxBeActive = (progress >= currentAttackData.hitboxStartTime && progress <= currentAttackData.hitboxEndTime);

            // 무기 스크립트가 존재할 때(맨손이 아닐 때)만 실행합니다!
            if (player.currentWeaponScript != null)
            {
                if (shouldHitboxBeActive)
                {
                    float baseDmg = player.unarmedBaseDamage; // 기본값은 맨손 데미지

                    ItemInstance equippedWeapon = EquipmentManager.Instance.currentWeapons[EquipmentManager.Instance.activeWeaponIndex];

                    // 장착된 무기 아이템이 있다면 그 무기의 스탯 보정 데미지를 가져옴
                    if (equippedWeapon != null && equippedWeapon.itemData != null)
                    {
                        baseDmg = equippedWeapon.GetCalculatedDamage(player.stats.strength, player.stats.dexterity);
                    }

                    if (player.currentWeaponScript != null)
                    {
                        player.currentWeaponScript.currentDamage = baseDmg * currentAttackData.damageMultiplier;
                        player.currentWeaponScript.EnableHitbox();
                    }
                }
                else
                {
                    // 콜라이더 끄기
                    player.currentWeaponScript.DisableHitbox();
                }
            }
        }

        // 콤보 전환 로직
        if (comboInputReceived && player.canNextAttack)
        {
            if (comboIndex + 1 < player.currentWeapon.comboAttacks.Length)
            {
                // 2. 다음 콤보로 넘어가기 전, 스테미나가 남아있는지 확인!
                
                if (player.stats.CurrentStamina > 0)
                {
                    comboIndex++;
                    isContinuingCombo = true;
                    stateMachine.ChangeState(this);
                    return;
                }
                else
                {
                    // 스테미나가 없으면 콤보 선입력을 무시 (공격이 안 나감)
                    comboInputReceived = false;
                }
            }
        }

        // 이동/회피 캔슬 로직
        if (player.canCancel)
        {
            // 구르기 선입력이 들어와 있다면 최우선으로 회피 발동!
            if (dodgeInputReceived)
            {
                // 회피 캔슬 시에도 스테미나 체크
                if (player.stats.CurrentStamina > 0)
                {
                    stateMachine.ChangeState(player.dashState);
                    return;
                }
                else
                {
                    dodgeInputReceived = false;
                }
            }

            if (player.moveInput != Vector2.zero)
            {
                stateMachine.ChangeState(player.moveState);
                return;
            }
        }

        if (player.moveInput == Vector2.zero && progress >= 0.95f)
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
            if (player.currentWeaponScript != null)
            {
                player.currentWeaponScript.DisableHitbox();
            }

            comboIndex = 0;
            player.animator.applyRootMotion = false;

        }
    }

    public override void OnDashInput()
    {
        dodgeInputReceived = true;
    }
}