using UnityEngine;

public class Humanoid_Boss_Run_State : BossBaseState
{
    private float stateTimer;
    private float maxRunTime = 6f; // 달리기 최대 시간
    private float targetAttackRange = 6f; // 달리다가 멈출 거리 (걷기보다 살짝 길게 주어 돌진기 연계 유도)

    public Humanoid_Boss_Run_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        stateTimer = 0f;
        boss.animator.CrossFadeInFixedTime("Run", 0.2f);
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(boss.transform.position, boss.target.position);

        //  종료 조건 1: 돌진/공격하기 좋은 거리에 도달
        if (distanceToPlayer <= targetAttackRange)
        {
            boss.ReturnToDecision();
            return;
        }

        //  종료 조건 2: 타임아웃
        if (stateTimer >= maxRunTime)
        {
            boss.ReturnToDecision();
            return;
        }

        // 이동 로직: 플레이어를 바라보며 최대 속도로 전진
        boss.RotateTowardsTarget();
        boss.MoveBoss(boss.transform.forward * boss.moveSpeed * Time.deltaTime);
    }
}