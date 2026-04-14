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

        if (distanceToPlayer <= enemy.attackRange)
        {
            if (Time.time >= enemy.lastAttackTime + enemy.attackCooldown)
            {
                enemyStateMachine.ChangeState(enemy.attackState);
            }
            else
            {
                enemyStateMachine.ChangeState(enemy.strafeState);
            }
            return;
        }
        enemy.MoveEnemy(enemy.transform.forward * enemy.moveSpeed * Time.deltaTime);
        //enemy.characterController.Move(enemy.transform.forward * enemy.moveSpeed * Time.deltaTime);
    }

    public override void Exit()
    {
    }

}
