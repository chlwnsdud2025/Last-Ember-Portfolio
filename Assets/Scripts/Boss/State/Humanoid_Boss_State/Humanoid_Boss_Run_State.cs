using UnityEngine;

public class Humanoid_Boss_Run_State : BossBaseState
{
    private float stateTimer;
    private float maxRunTime = 6f; // �޸��� �ִ� �ð�
    private float targetAttackRange = 6f; // �޸��ٰ� ���� �Ÿ� (�ȱ⺸�� ��¦ ��� �־� ������ ���� ����)

    public Humanoid_Boss_Run_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        stateTimer = 0f;
        boss.animator.CrossFadeInFixedTime("Run", 0.2f);
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(boss.transform.position, boss.target.position);

        //  ���� ���� 1: ����/�����ϱ� ���� �Ÿ��� ����
        if (distanceToPlayer <= targetAttackRange)
        {
            boss.ReturnToDecision();
            return;
        }

        //  ���� ���� 2: Ÿ�Ӿƿ�
        if (stateTimer >= maxRunTime)
        {
            boss.ReturnToDecision();
            return;
        }

        // �̵� ����: �÷��̾ �ٶ󺸸� �ִ� �ӵ��� ����
        boss.RotateTowardsTarget();
        boss.MoveBoss(boss.transform.forward * boss.moveSpeed * Time.deltaTime);
    }
}