using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{

    private readonly int idleAnimHash = Animator.StringToHash("Idle");

    public EnemyIdleState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        //Debug.Log("Enemy 상태: Idle");
        enemy.animator.CrossFadeInFixedTime(idleAnimHash, 0.1f);
    }

    public override void Update()
    {
        Collider[] hitPlayers = Physics.OverlapSphere(enemy.transform.position, enemy.detectionRadius, enemy.playerLayer);

        if (hitPlayers.Length > 0)
        {
            Transform potentialTarget = hitPlayers[0].transform;

            Vector3 dirToTarget = (potentialTarget.position - enemy.transform.position).normalized;

            //  적의 정면(forward)과 플레이어 사이의 각도가 시야각(viewAngle)의 절반 이내인가?
            if (Vector3.Angle(enemy.transform.forward, dirToTarget) < enemy.viewAngle / 2)
            {
                // 적의 눈과 플레이어 사이에 벽(obstacleLayer)이 있는가?

                // 적과 플레이어의 눈높이(약 1.5f)를 맞춰서 레이저를 쏩니다
                Vector3 eyePosition = enemy.transform.position + Vector3.up * 1.5f;
                Vector3 targetPosition = potentialTarget.position + Vector3.up * 1.5f;
                Vector3 rayDir = (targetPosition - eyePosition).normalized;
                float distanceToTarget = Vector3.Distance(eyePosition, targetPosition);

                // 레이저를 쐈는데 벽(Obstacle)에 맞지 않았다면? -> 완벽히 시야에 들어옴!
                if (!Physics.Raycast(eyePosition, rayDir, distanceToTarget, enemy.obstacleLayer))
                {
                    enemy.target = potentialTarget;
                    enemyStateMachine.ChangeState(enemy.chaseState);
                }
                
            }
        }
    }

    public override void Exit() { }
}
