using UnityEngine;

public class EnemyStrafeState : EnemyBaseState
{
    private readonly int strafeAnimHash = Animator.StringToHash("Strafe");
    private int strafeDirection;

    public EnemyStrafeState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        //Debug.Log("Enemy: Strafe");
        enemy.animator.CrossFadeInFixedTime(strafeAnimHash, 0.1f);

        //회전 방향 설정
        strafeDirection = Random.Range(0, 2) == 0 ? 1 : -1;
        enemy.animator.SetFloat("strafeDirection", strafeDirection);
    }

    public override void Update()
    {
        if (enemy.target == null)
        {
            enemyStateMachine.ChangeState(enemy.idleState);
            return;
        }

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.target.position);

        // 사거리를 벗어날시 chase 전환
        if (distanceToPlayer > enemy.strafeRange * 1.2f)
        {
            enemyStateMachine.ChangeState(enemy.chaseState);
            return;
        }

        // -------------------------------------------------------------
        //  견제 중 쿨타임 회복 시 행동 판단
        // -------------------------------------------------------------
        if (Time.time >= enemy.lastAttackTime + enemy.attackCooldown)
        {
            EnemyAttackState nextAttack = enemy.GetBestAttack(distanceToPlayer);

            if (nextAttack != null)
            {
                // 돌다가 거리가 맞아서 공격할 수 있으면 즉시 공격!
                enemyStateMachine.ChangeState(nextAttack);
                return;
            }
            else
            {
                //  [핵심] 쿨타임은 다 찼는데 거리가 멀어서 때릴 기술이 없다면?
                // 게걸음을 멈추고 거리를 좁히기 위해 다시 Chase 상태로 돌아갑니다!
                enemyStateMachine.ChangeState(enemy.chaseState);
                return;
            }
        }




        Vector3 dirToTarget = (enemy.target.position - enemy.transform.position).normalized;
        dirToTarget.y = 0f;

        if (dirToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        Vector3 strafeMove = enemy.transform.right * strafeDirection * enemy.strafeSpeed;
        enemy.MoveEnemy(strafeMove * Time.deltaTime);
        //enemy.characterController.Move(strafeMove * Time.deltaTime);
    }
}
