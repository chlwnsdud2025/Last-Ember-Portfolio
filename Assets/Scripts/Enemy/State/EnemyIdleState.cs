using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{

    private readonly int idleAnimHash = Animator.StringToHash("Idle");

    public EnemyIdleState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        //Debug.Log("Enemy ����: Idle");
        enemy.animator.CrossFadeInFixedTime(idleAnimHash, 0.1f);
    }

    public override void Update()
    {
        Collider[] hitPlayers = Physics.OverlapSphere(enemy.transform.position, enemy.detectionRadius, enemy.playerLayer);

        if (hitPlayers.Length > 0)
        {
            Transform potentialTarget = hitPlayers[0].transform;

            Vector3 dirToTarget = (potentialTarget.position - enemy.transform.position).normalized;

            //  ���� ����(forward)�� �÷��̾� ������ ������ �þ߰�(viewAngle)�� ���� �̳��ΰ�?
            if (Vector3.Angle(enemy.transform.forward, dirToTarget) < enemy.viewAngle / 2)
            {
                // ���� ���� �÷��̾� ���̿� ��(obstacleLayer)�� �ִ°�?

                // ���� �÷��̾��� ������(�� 1.5f)�� ���缭 �������� ���ϴ�
                Vector3 eyePosition = enemy.transform.position + Vector3.up * 1.5f;
                Vector3 targetPosition = potentialTarget.position + Vector3.up * 1.5f;
                Vector3 rayDir = (targetPosition - eyePosition).normalized;
                float distanceToTarget = Vector3.Distance(eyePosition, targetPosition);

                // �������� ���µ� ��(Obstacle)�� ���� �ʾҴٸ�? -> �Ϻ��� �þ߿� ����!
                if (!Physics.Raycast(eyePosition, rayDir, distanceToTarget, enemy.obstacleLayer))
                {
                    enemy.target = potentialTarget;
                    enemyStateMachine.ChangeState(enemy.chaseState);
                }
                
            }
        }
    }

    public override void Exit() { }
}
