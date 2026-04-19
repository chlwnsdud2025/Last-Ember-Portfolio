using UnityEngine;

public class Humanoid_Boss_Attack_State : BossBaseState
{
    public BossAttack_SO_Data currentAttackData;

    private bool isHitboxActive = false;
    private bool hasTransitioned = false;

    public Humanoid_Boss_Attack_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        Debug.Log($"Attack 진입: {currentAttackData?.attackName}"); // 디버그를 더 명확하게!

        if (currentAttackData == null || currentAttackData.attackClip == null)
        {
            boss.ReturnToDecision();
            return;
        }

        isHitboxActive = false;
        hasTransitioned = false;

        //  변경 1: 루트 모션과 배속을 SO 데이터에 맞게 세팅!
        boss.animator.applyRootMotion = currentAttackData.useRootMotion;
        boss.animator.speed = currentAttackData.animationSpeedMultiplier;

        boss.overrideController["Dummy_Attack"] = currentAttackData.attackClip;

        //  변경 2: 하드코딩된 0.2f 대신 SO에 있는 transitionDuration 사용
        boss.animator.CrossFadeInFixedTime("Attack", currentAttackData.transitionDuration);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = boss.animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("Attack") || boss.animator.IsInTransition(0))
        {
            return;
        }

        float progress = stateInfo.normalizedTime;

        //  단순 boolean(trackTarget) 대신, Enum을 이용한 세밀한 추적 로직!
        if (currentAttackData.trackingType == RotationTrackingType.Continuous)
        {
            // 끝날 때까지 쳐다봄 (SO에 설정한 turnSpeed 속도 적용!)
            boss.RotateTowardsTarget(currentAttackData.turnSpeed);
        }
        else if (currentAttackData.trackingType == RotationTrackingType.OnlyStartup && progress <= currentAttackData.stopTrackingTime)
        {
            // 선딜(지정된 시간)까지만 쳐다봄 (SO에 설정한 turnSpeed 속도 적용!)
            boss.RotateTowardsTarget(currentAttackData.turnSpeed);
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
                Debug.Log($"{currentAttackData.attackName} 판정 꺼짐.");
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

        // 다음 상태(Walk, Idle 등)가 고장 나지 않도록 배속 원상복구!
        boss.animator.speed = 1.0f;
    }
}