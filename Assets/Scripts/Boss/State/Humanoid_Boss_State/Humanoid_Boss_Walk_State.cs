using UnityEngine;

public class Humanoid_Boss_Walk_State : BossBaseState
{
    private float stateTimer;
    private float maxWalkTime = 4f; // 최대 4초까지만 걷기
    private float targetAttackRange = 5f; // 이 거리 안에 들어오면 공격을 위해 멈춤

    public Humanoid_Boss_Walk_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        stateTimer = 0f;
        boss.animator.CrossFadeInFixedTime("Walk", 0.2f);
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(boss.transform.position, boss.target.position);

        //  종료 조건 1: 때리기 딱 좋은 거리까지 다가왔을 때
        if (distanceToPlayer <= targetAttackRange)
        {
            boss.ReturnToDecision();
            return;
        }

        //  종료 조건 2: 너무 오래 걸었을 때 (장애물에 막혔거나 플레이어가 계속 도망침)
        if (stateTimer >= maxWalkTime)
        {
            boss.ReturnToDecision();
            return;
        }

        // 이동 로직: 플레이어를 바라보며 기본 속도의 절반(0.5f)으로 천천히 전진
        boss.RotateTowardsTarget();
        boss.MoveBoss(boss.transform.forward * (boss.moveSpeed * 0.5f) * Time.deltaTime);
    }
}