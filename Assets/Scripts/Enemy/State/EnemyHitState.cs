using UnityEngine;

public class EnemyHitState : EnemyBaseState
{
    private readonly int hitAnimHash = Animator.StringToHash("Hit");

    public EnemyHitState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        //Debug.Log("Enemy 상태: Hit (경직!)");

        // 피격 모션 재생 (이전 애니메이션 무시하고 강제로 0초부터 시작)
        enemy.animator.CrossFadeInFixedTime(hitAnimHash, 0.1f, 0, 0f);

        // 만약 맞는 애니메이션에 뒤로 밀려나는 루트 모션이 있다면 켭니다.
        enemy.animator.applyRootMotion = true;
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);

        // 현재 실제로 "Hit" 모션이 재생 중인지 확인
        if (stateInfo.shortNameHash == hitAnimHash && !enemy.animator.IsInTransition(0))
        {
            // 경직이 풀릴 시
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
