using UnityEngine;

public class Humanoid_Boss_Attack_State : BossBaseState
{
    public BossAttack_SO_Data currentAttackData;

    private bool isHitboxActive = false;
    private bool hasTransitioned = false;

    public Humanoid_Boss_Attack_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Attack");
        if (currentAttackData == null || currentAttackData.attackClip == null)
        {
            boss.ReturnToDecision();
            return;
        }

        isHitboxActive = false;
        hasTransitioned = false;
        boss.animator.applyRootMotion = true;

        
        boss.overrideController["Dummy_Attack"] = currentAttackData.attackClip;

        boss.animator.CrossFadeInFixedTime("Attack", 0.2f);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = boss.animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("Attack") || boss.animator.IsInTransition(0))
        {
            return;
        }

        float progress = stateInfo.normalizedTime;

        // 이제 '진짜' Attack 애니메이션의 진행도만 읽게 됩니다.
        if (currentAttackData.trackTarget && progress < currentAttackData.hitboxStartTime)
        {
            boss.RotateTowardsTarget();
        }

        HandleHitboxTiming(progress);
        HandleStateTransition(progress);
    }

    private void HandleHitboxTiming(float progress)
    {
        if (progress >= currentAttackData.hitboxStartTime && progress <= currentAttackData.hitboxEndTime)
        {
            if (!isHitboxActive)
            {
                isHitboxActive = true;
                Debug.Log($"{currentAttackData.attackName} 판정 켜짐!");
            }
        }
        else
        {
            if (isHitboxActive)
            {
                isHitboxActive = false;
                //Debug.Log($"{currentAttackData.attackName} 판정 꺼짐.");
            }
        }
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
    }
}