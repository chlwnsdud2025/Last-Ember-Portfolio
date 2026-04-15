using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{

    private int randomAttackInt = 1;

    int attackAnimHash;
    public EnemyAttackState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
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

        // �ڿ� ����: ���� ����� 95% �̻� ����Ǹ� ���� ��!
        if (isPlayingAttack && !enemy.animator.IsInTransition(0) && stateInfo.normalizedTime >= 0.95f)
        {
            // ������ ������ ������ ������(Strafe)�� �Ѿ�� ��Ÿ���� ��ٸ��ϴ�.
            enemyStateMachine.ChangeState(enemy.strafeState);
        }
    }

    public override void Exit()
    {
        // ������ ������ ��Ʈ ����� ���� �����ϰ� �̲������ ���� ����
        enemy.animator.applyRootMotion = false;
        enemy.currentWeaponScript.DisableHitbox();
    }

}
