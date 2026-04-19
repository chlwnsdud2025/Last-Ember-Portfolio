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

        //ȸ�� ���� ����
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

        // �÷��̾���� �Ÿ� ���
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.target.position);

        // ��Ÿ��� ����� chase ��ȯ
        if (distanceToPlayer > enemy.strafeRange * 1.2f)
        {
            enemyStateMachine.ChangeState(enemy.chaseState);
            return;
        }

        // -------------------------------------------------------------
        //  ���� �� ��Ÿ�� ȸ�� �� �ൿ �Ǵ�
        // -------------------------------------------------------------
        if (Time.time >= enemy.lastAttackTime + enemy.attackCooldown)
        {
            EnemyAttackState nextAttack = enemy.GetBestAttack(distanceToPlayer);

            if (nextAttack != null)
            {
                // ���ٰ� �Ÿ��� �¾Ƽ� ������ �� ������ ��� ����!
                enemyStateMachine.ChangeState(nextAttack);
                return;
            }
            else
            {
                //  [�ٽ�] ��Ÿ���� �� á�µ� �Ÿ��� �־ ���� ����� ���ٸ�?
                // �԰����� ���߰� �Ÿ��� ������ ���� �ٽ� Chase ���·� ���ư��ϴ�!
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
