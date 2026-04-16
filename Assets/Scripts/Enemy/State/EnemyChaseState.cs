using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    private readonly int chaseAnimHash = Animator.StringToHash("Chase"); 

    public EnemyChaseState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        //Debug.Log("Enemy 상태: Chase (추적 시작!)");
        enemy.animator.CrossFadeInFixedTime(chaseAnimHash, 0.1f);
    }

    public override void Update()
    {
        //Idle 전환
        if (enemy.target == null)
        {
            enemyStateMachine.ChangeState(enemy.idleState);
            return;
        }

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.target.position);

        // 어그로 풀림 조건 -> 탐지 거리의 1.5배 이상 멀어지면 복귀
        if (distanceToPlayer > enemy.detectionRadius * 1.5f)
        {
            enemy.target = null;
            enemyStateMachine.ChangeState(enemy.idleState);
            return;
        }

        
        Vector3 dirToTarget = (enemy.target.position - enemy.transform.position).normalized;
        dirToTarget.y = 0f;

        // 회전
        if (dirToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, 10f * Time.deltaTime);
        }


        if (Time.time >= enemy.lastAttackTime + enemy.attackCooldown)
        {
            EnemyAttackState nextAttack = enemy.GetBestAttack(distanceToPlayer);

            if (nextAttack != null)
            {
                // 지금 당장 때릴 수 있는 공격이 있으면 발동!
                enemyStateMachine.ChangeState(nextAttack);
                return;
            }
            // 여기서 else로 Strafe를 부르지 않습니다! 
            // 쿨타임은 찼는데 거리가 안 닿는다면, 아래쪽의 MoveEnemy가 실행되어 계속 다가갑니다.
        }
        // 2. 공격 쿨타임이 돌고 있을 때 (시간을 벌어야 함)
        else
        {
            // 아직 때리지도 못하는데 플레이어와 충분히 가깝다면(strafeRange), 빙빙 돌면서 시간을 끕니다.
            if (distanceToPlayer <= enemy.strafeRange)
            {
                enemyStateMachine.ChangeState(enemy.strafeState);
                return;
            }
        }
        enemy.MoveEnemy(enemy.transform.forward * enemy.moveSpeed * Time.deltaTime);
        //enemy.characterController.Move(enemy.transform.forward * enemy.moveSpeed * Time.deltaTime);
    }

    public override void Exit()
    {
    }

}
