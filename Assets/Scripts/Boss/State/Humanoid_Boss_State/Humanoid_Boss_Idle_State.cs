using UnityEngine;

public class Humanoid_Boss_Idle_State : BossBaseState
{
    private float stateTimer;
    private float idleDuration; // 쉴 시간

    public Humanoid_Boss_Idle_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        stateTimer = 0f;
        // 0.5초에서 1.5초 사이로 랜덤하게 숨을 고릅니다 (기계적인 느낌 방지)
        idleDuration = Random.Range(0.5f, 1.5f);

        boss.animator.CrossFadeInFixedTime("Idle", 0.2f);
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;

        // 가만히 서 있더라도 시선은 플레이어를 향해 묵직하게 고정합니다.
        boss.RotateTowardsTarget();

        //  종료 조건: 쉬는 시간이 끝나면 다시 생각하러 갑니다.
        if (stateTimer >= idleDuration)
        {
            boss.ReturnToDecision();
        }
    }
}