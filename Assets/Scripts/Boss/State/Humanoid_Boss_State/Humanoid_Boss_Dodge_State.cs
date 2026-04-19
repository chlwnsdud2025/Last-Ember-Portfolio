using UnityEngine;

public class Humanoid_Boss_Dodge_State : BossBaseState
{
    //  �ִϸ����� �Ķ���� �� ���� �ؽð� (�̸� ĳ���Ͽ� ���� �ش�ȭ)
    private readonly int dodgeXHash = Animator.StringToHash("DodgeX");
    private readonly int dodgeYHash = Animator.StringToHash("DodgeY");
    private readonly int dodgeStateHash = Animator.StringToHash("Dodge"); // ���� �̸� �ؽ�

    public Humanoid_Boss_Dodge_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        boss.animator.applyRootMotion = true;

        float distanceToPlayer = Vector3.Distance(boss.transform.position, boss.target.position);
        float targetX = 0f;
        float targetY = 0f;



        if (distanceToPlayer < 3f)
        {
            targetY = -1f; // �齺��
        }
        else if (distanceToPlayer > 10f)
        {
            targetY = 1f;
        }
        else if (distanceToPlayer >= 3f && distanceToPlayer <= 10f)
        {
            targetX = Random.value > 0.5f ? 1f : -1f; // ���̵彺��
        }

        boss.animator.SetFloat(dodgeXHash, targetX);
        boss.animator.SetFloat(dodgeYHash, targetY);

        boss.animator.CrossFadeInFixedTime("Dodge", 0.1f);
    }

    public override void Update()
    {
        boss.RotateTowardsTarget();

        //  �������� ���̵�� ����: ���൵�� �����ͼ� �Ǵ�!
        AnimatorStateInfo stateInfo = boss.animator.GetCurrentAnimatorStateInfo(0);

        // 1. ���� ��� ���� ���°� �Ϻ��ϰ� 'Dodge'���� Ȯ�� (Ʈ������ ���� ����)
        // 2. �ִϸ��̼� ���൵�� 95% �̻� ����Ǿ����� Ȯ�� (1.0���� �� ���߸� 1������ Ƣ�� ���� ����)
        if (stateInfo.shortNameHash == dodgeStateHash && stateInfo.normalizedTime >= 0.95f)
        {
            boss.ReturnToDecision();
        }
    }

    public override void Exit()
    {
        boss.animator.applyRootMotion = false;
    }
}