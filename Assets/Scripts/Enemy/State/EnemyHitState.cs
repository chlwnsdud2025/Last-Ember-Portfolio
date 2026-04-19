using UnityEngine;

public class EnemyHitState : EnemyBaseState
{
    private readonly int hitAnimHash = Animator.StringToHash("Hit");

    public EnemyHitState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {

        enemy.animator.CrossFadeInFixedTime(hitAnimHash, 0.1f, 0, 0f);

        enemy.animator.applyRootMotion = true;
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.shortNameHash == hitAnimHash && !enemy.animator.IsInTransition(0))
        {
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
