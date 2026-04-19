using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    private readonly int chaseAnimHash = Animator.StringToHash("Chase"); 

    public EnemyChaseState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        //Debug.Log("Enemy ����: Chase (���� ����!)");
        enemy.animator.CrossFadeInFixedTime(chaseAnimHash, 0.1f);
    }

    public override void Update()
    {
        //Idle ��ȯ
        if (enemy.target == null)
        {
            enemyStateMachine.ChangeState(enemy.idleState);
            return;
        }

        // �÷��̾���� �Ÿ� ���
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.target.position);

        // ��׷� Ǯ�� ���� -> Ž�� �Ÿ��� 1.5�� �̻� �־����� ����
        if (distanceToPlayer > enemy.detectionRadius * 1.5f)
        {
            enemy.target = null;
            enemyStateMachine.ChangeState(enemy.idleState);
            return;
        }

        
        Vector3 dirToTarget = (enemy.target.position - enemy.transform.position).normalized;
        dirToTarget.y = 0f;

        // ȸ��
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
                // ���� ���� ���� �� �ִ� ������ ������ �ߵ�!
                enemyStateMachine.ChangeState(nextAttack);
                return;
            }
            // ���⼭ else�� Strafe�� �θ��� �ʽ��ϴ�! 
            // ��Ÿ���� á�µ� �Ÿ��� �� ��´ٸ�, �Ʒ����� MoveEnemy�� ����Ǿ� ��� �ٰ����ϴ�.
        }
        // 2. ���� ��Ÿ���� ���� ���� �� (�ð��� ����� ��)
        else
        {
            // ���� �������� ���ϴµ� �÷��̾�� ����� �����ٸ�(strafeRange), ���� ���鼭 �ð��� ���ϴ�.
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
