using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{

    private int randomAttackInt = 1;

    int attackAnimHash;
    public EnemyAttackState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        //Debug.Log("Enemy 상태: Attack");
        randomAttackInt = Random.Range(1, 4);
        enemy.lastAttackTime = Time.time;

        enemy.animator.applyRootMotion = true;

        attackAnimHash = Animator.StringToHash($"Attack{randomAttackInt}");

       
        enemy.animator.CrossFadeInFixedTime(attackAnimHash, 0.2f, 0, 0f);

        enemy.currentWeaponScript.EnableHitbox();

    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);

        bool isPlayingAttack = stateInfo.shortNameHash == attackAnimHash;

        
        if (enemy.animator.IsInTransition(0) || (isPlayingAttack && stateInfo.normalizedTime <= 0.3f))
        {
            if (enemy.target != null)
            {
                Vector3 dirToTarget = (enemy.target.position - enemy.transform.position).normalized;
                dirToTarget.y = 0f;
                if (dirToTarget != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
                    enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRot, 15f * Time.deltaTime);
                }
            }
        }

        // 자연 종료: 공격 모션이 95% 이상 진행되면 공격 끝!
        if (isPlayingAttack && !enemy.animator.IsInTransition(0) && stateInfo.normalizedTime >= 0.95f)
        {
            // 공격이 끝나면 무조건 간보기(Strafe)로 넘어가서 쿨타임을 기다립니다.
            enemyStateMachine.ChangeState(enemy.strafeState);
        }
    }

    public override void Exit()
    {
        // 공격이 끝나면 루트 모션을 꺼서 엉뚱하게 미끄러지는 것을 방지
        enemy.animator.applyRootMotion = false;
        enemy.currentWeaponScript.DisableHitbox();
    }

}
