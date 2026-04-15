using UnityEngine;

public abstract class EnemyBaseState
{
    protected EnemyController enemy;
    protected EnemyStateMachine enemyStateMachine;

    public EnemyBaseState( EnemyController enemy, EnemyStateMachine enemyStateMachine)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }



    public virtual void OnAttackInput() { enemyStateMachine.ChangeState(enemy.hitsState); }

}
