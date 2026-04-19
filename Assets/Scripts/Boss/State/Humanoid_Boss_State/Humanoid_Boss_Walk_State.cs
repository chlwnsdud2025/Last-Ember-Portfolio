using UnityEngine;

public class Humanoid_Boss_Walk_State : BossBaseState
{
    private float stateTimer;
    private float maxWalkTime = 4f; // �ִ� 4�ʱ����� �ȱ�
    private float targetAttackRange = 5f; // �� �Ÿ� �ȿ� ������ ������ ���� ����

    public Humanoid_Boss_Walk_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        stateTimer = 0f;
        boss.animator.CrossFadeInFixedTime("Walk", 0.2f);
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(boss.transform.position, boss.target.position);

        //  ���� ���� 1: ������ �� ���� �Ÿ����� �ٰ����� ��
        if (distanceToPlayer <= targetAttackRange)
        {
            boss.ReturnToDecision();
            return;
        }

        //  ���� ���� 2: �ʹ� ���� �ɾ��� �� (��ֹ��� �����ų� �÷��̾ ��� ����ħ)
        if (stateTimer >= maxWalkTime)
        {
            boss.ReturnToDecision();
            return;
        }

        // �̵� ����: �÷��̾ �ٶ󺸸� �⺻ �ӵ��� ����(0.5f)���� õõ�� ����
        boss.RotateTowardsTarget();
        boss.MoveBoss(boss.transform.forward * (boss.moveSpeed * 0.5f) * Time.deltaTime);
    }
}