using UnityEngine;

public class EnemyStateMachine
{
    public EnemyBaseState enemyCurrentState { get; private set; }


    public void Initialize(EnemyBaseState startingState)
    {
        enemyCurrentState = startingState;
        enemyCurrentState.Enter();
    }
    public void ChangeState(EnemyBaseState newState)
    {
        enemyCurrentState.Exit();
        enemyCurrentState = newState;
        enemyCurrentState.Enter();
    }
    public void Update()
    {
        if (enemyCurrentState != null) enemyCurrentState.Update();

    }
}
