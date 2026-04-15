using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    private int randomAttackInt = 1;
    private int attackAnimHash;

    // 공격 중 회전 및 이동 제어를 위한 변수
    private bool isRotationLocked = false;

    public EnemyAttackState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        randomAttackInt = Random.Range(1, 4);
        enemy.lastAttackTime = Time.time;

        // 루트 모션 활성화 (애니메이션의 전진 값을 사용)
        enemy.animator.applyRootMotion = true;
        isRotationLocked = false;

        attackAnimHash = Animator.StringToHash($"Attack{randomAttackInt}");
        enemy.animator.CrossFadeInFixedTime(attackAnimHash, 0.2f, 0, 0f);

        // 히트박스는 애니메이션 이벤트로 켜는 것이 좋지만, 
        // 현재 구조 유지를 위해 여기서 켭니다. (이후 이벤트 방식으로 수정을 추천)
        enemy.currentWeaponScript.EnableHitbox();
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        bool isPlayingAttack = stateInfo.shortNameHash == attackAnimHash;

        if (isPlayingAttack && !enemy.animator.IsInTransition(0))
        {
            // 1. 공격 준비 단계 (전체 애니메이션의 30% 이전): 플레이어를 향해 정교하게 회전
            if (stateInfo.normalizedTime <= 0.3f)
            {
                RotateTowardsTarget(20f); // 추적 속도를 좀 더 높임
            }
            // 2. 공격 발동 단계: 회전 고정 (소울류 특유의 묵직한 공격 방향 고정)
            else if (stateInfo.normalizedTime > 0.3f && !isRotationLocked)
            {
                isRotationLocked = true;
            }

            // 3. 종료 단계: 상태 전환
            if (stateInfo.normalizedTime >= 0.95f)
            {
                enemyStateMachine.ChangeState(enemy.strafeState);
            }
        }
    }

    private void RotateTowardsTarget(float speed)
    {
        if (enemy.target != null)
        {
            Vector3 dirToTarget = (enemy.target.position - enemy.transform.position).normalized;
            dirToTarget.y = 0f;
            if (dirToTarget != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRot, speed * Time.deltaTime);
            }
        }
    }

    public override void Exit()
    {
        enemy.animator.applyRootMotion = false;
        enemy.currentWeaponScript.DisableHitbox();
        isRotationLocked = false;
    }
}