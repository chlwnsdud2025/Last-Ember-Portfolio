using UnityEngine;

public class EnemyHitState : EnemyBaseState
{
    private readonly int hitAnimHash = Animator.StringToHash("Hit");

    public EnemyHitState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        //Debug.Log("Enemy ����: Hit (����!)");

        // �ǰ� ��� ��� (���� �ִϸ��̼� �����ϰ� ������ 0�ʺ��� ����)
        enemy.animator.CrossFadeInFixedTime(hitAnimHash, 0.1f, 0, 0f);

        // ���� �´� �ִϸ��̼ǿ� �ڷ� �з����� ��Ʈ ����� �ִٸ� �մϴ�.
        enemy.animator.applyRootMotion = true;
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);

        // ���� ������ "Hit" ����� ��� ������ Ȯ��
        if (stateInfo.shortNameHash == hitAnimHash && !enemy.animator.IsInTransition(0))
        {
            // ������ Ǯ�� ��
            if (stateInfo.normalizedTime >= 0.9f)
            {
                
                enemyStateMachine.ChangeState(enemy.chaseState);
            }
        }

        enemy.MoveEnemy(Vector3.zero);
    }

    public override void Exit()
    {
        enemy.animator.applyRootMotion = false;
    }

}
