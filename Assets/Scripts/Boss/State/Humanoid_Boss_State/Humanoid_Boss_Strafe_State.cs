using UnityEngine;

public class Humanoid_Boss_Strafe_State : BossBaseState
{
    private float stateTimer;
    private float strafeDuration;
    private int strafeDirection; // 1�̸� ����, -1�̸� ����

    public Humanoid_Boss_Strafe_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        stateTimer = 0f;
        strafeDuration = Random.Range(1.5f, 3.0f); // 1.5�� ~ 3�� ���� ���� ���ϴ�.

        // ������ �� ��, �� �� ��� �������� ���� 50% Ȯ���� �����մϴ�.
        strafeDirection = Random.value > 0.5f ? 1 : -1;
        boss.animator.SetFloat("strafeDirection", strafeDirection);
        boss.animator.CrossFadeInFixedTime("Strafe", 0.2f);
        // ��: �ִϸ����Ϳ� ���� �ȱ�, ���� �ȱⰡ ���� �ִٸ� strafeDirection�� ���� �б� ó���ϸ� �˴ϴ�.
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(boss.transform.position, boss.target.position);

        //  ���� ���� 1: ���� �� ������ �ٽ� ����/�̵� �Ǵ�
        if (stateTimer >= strafeDuration)
        {
            boss.ReturnToDecision();
            return;
        }

        //  ���� ���� 2: ���� ���� �ִµ� �÷��̾ ���ڱ� Ȯ �־����ų� Ȯ �ٰ��� ��
        // (��Ȳ�� �������Ƿ� ���⸦ ���߰� �ٽ� �����մϴ�)
        if (distanceToPlayer > 8f || distanceToPlayer < 1.5f)
        {
            boss.ReturnToDecision();
            return;
        }

        // �̵� ����: �÷��̾ �ȹٷ� �Ĵٺ� ���¿��� '������(transform.right)' �̵��մϴ�.
        boss.RotateTowardsTarget();
        Vector3 moveDir = boss.transform.right * strafeDirection;
        boss.MoveBoss(moveDir * (boss.moveSpeed * 0.4f) * Time.deltaTime);
    }
}