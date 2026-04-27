using UnityEngine;

public class Humanoid_Boss_Attack_State : BossBaseState
{
    public BossAttack_SO_Data currentAttackData;

    private bool isHitboxActive = false;
    private bool hasTransitioned = false;
    private bool hasSpawned = false;

    public Humanoid_Boss_Attack_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {

        if (boss.hitboxManager == null)
        {
        }

        if (currentAttackData == null || currentAttackData.attackClip == null)
        {
            boss.ReturnToDecision();
            return;
        }

        isHitboxActive = false;
        hasTransitioned = false;
        hasSpawned = false;

        boss.animator.applyRootMotion = currentAttackData.useRootMotion;
        boss.animator.speed = currentAttackData.animationSpeedMultiplier;

        boss.overrideController["Dummy_Attack"] = currentAttackData.attackClip;
        boss.animator.CrossFadeInFixedTime("Attack", currentAttackData.transitionDuration);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = boss.animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("Attack"))
        {
            return;
        }

        if (boss.animator.IsInTransition(0)) return;

        float progress = stateInfo.normalizedTime;


        // 타겟 추적(회전) 로직
        if (currentAttackData.trackingType == RotationTrackingType.Continuous)
        {
            boss.RotateTowardsTarget(currentAttackData.turnSpeed);
        }
        else if (currentAttackData.trackingType == RotationTrackingType.OnlyStartup && progress <= currentAttackData.stopTrackingTime)
        {
            boss.RotateTowardsTarget(currentAttackData.turnSpeed);
        }

        HandleAttackAction(progress);
        HandleStateTransition(progress);
    }

    private void HandleAttackAction(float progress)
    {
        if (currentAttackData.attackMethod == AttackMethod.Melee)
        {
            if (progress >= currentAttackData.hitboxStartTime && progress <= currentAttackData.hitboxEndTime)
            {
                if (!isHitboxActive)
                {
                    isHitboxActive = true;

                    if (boss.hitboxManager != null)
                    {
                        boss.hitboxManager.currentDamage = currentAttackData.damage;
                        boss.hitboxManager.EnableHitbox(currentAttackData.targetHitbox);
                    }
                }
            }
            else
            {
                if (isHitboxActive)
                {
                    isHitboxActive = false;

                    if (boss.hitboxManager != null) boss.hitboxManager.DisableAllHitboxes();
                }
            }
        }
        else if (currentAttackData.attackMethod == AttackMethod.SpawnPrefab)
        {
            if (progress >= currentAttackData.hitboxStartTime && !hasSpawned)
            {
                hasSpawned = true;
                SpawnAttackPrefab();
            }
        }
    }

    private void SpawnAttackPrefab()
    {
        if (currentAttackData.attackPrefab == null) return;
        Vector3 spawnPos = currentAttackData.spawnOnTarget && boss.target != null ? boss.target.position : boss.transform.position + boss.transform.TransformDirection(currentAttackData.spawnOffset);
        Object.Instantiate(currentAttackData.attackPrefab, spawnPos, boss.transform.rotation);
    }

    private void HandleStateTransition(float progress)
    {
        if (hasTransitioned) return;
        if (currentAttackData.nextCombo != null && progress >= currentAttackData.comboEnableTime)
        {
            hasTransitioned = true;
            currentAttackData = currentAttackData.nextCombo;
            stateMachine.ChangeState(this);
        }
        else if (progress >= currentAttackData.attackEndTime)
        {
            hasTransitioned = true;
            boss.ReturnToDecision();
        }
    }

    public override void Exit()
    {
        boss.animator.applyRootMotion = false;
        isHitboxActive = false;
        boss.animator.speed = 1.0f;
        if (boss.hitboxManager != null) boss.hitboxManager.DisableAllHitboxes();
    }
}