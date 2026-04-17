using UnityEngine;

public class Humanoid_Boss_Strafe_State : BossBaseState
{
    private float stateTimer;
    private float strafeDuration;
    private int strafeDirection; // 1이면 우측, -1이면 좌측

    public Humanoid_Boss_Strafe_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        stateTimer = 0f;
        strafeDuration = Random.Range(1.5f, 3.0f); // 1.5초 ~ 3초 동안 빙빙 돕니다.

        // 시작할 때 좌, 우 중 어느 방향으로 돌지 50% 확률로 결정합니다.
        strafeDirection = Random.value > 0.5f ? 1 : -1;
        boss.animator.SetFloat("strafeDirection", strafeDirection);
        boss.animator.CrossFadeInFixedTime("Strafe", 0.2f);
        // 팁: 애니메이터에 우측 걷기, 좌측 걷기가 따로 있다면 strafeDirection에 따라 분기 처리하면 됩니다.
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(boss.transform.position, boss.target.position);

        //  종료 조건 1: 간을 다 봤으면 다시 공격/이동 판단
        if (stateTimer >= strafeDuration)
        {
            boss.ReturnToDecision();
            return;
        }

        //  종료 조건 2: 빙빙 돌고 있는데 플레이어가 갑자기 확 멀어지거나 확 다가올 때
        // (상황이 변했으므로 돌기를 멈추고 다시 생각합니다)
        if (distanceToPlayer > 8f || distanceToPlayer < 1.5f)
        {
            boss.ReturnToDecision();
            return;
        }

        // 이동 로직: 플레이어를 똑바로 쳐다본 상태에서 '옆으로(transform.right)' 이동합니다.
        boss.RotateTowardsTarget();
        Vector3 moveDir = boss.transform.right * strafeDirection;
        boss.MoveBoss(moveDir * (boss.moveSpeed * 0.4f) * Time.deltaTime);
    }
}