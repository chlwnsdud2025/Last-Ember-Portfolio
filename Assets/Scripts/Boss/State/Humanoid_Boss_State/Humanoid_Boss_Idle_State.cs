using UnityEngine;

public class Humanoid_Boss_Idle_State : BossBaseState
{
    private float stateTimer;
    private float idleDuration; // �� �ð�

    public Humanoid_Boss_Idle_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        stateTimer = 0f;
        // 0.5�ʿ��� 1.5�� ���̷� �����ϰ� ���� ����ϴ� (������� ���� ����)
        idleDuration = Random.Range(0.5f, 1.5f);

        boss.animator.CrossFadeInFixedTime("Idle", 0.2f);
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;

        // ������ �� �ִ��� �ü��� �÷��̾ ���� �����ϰ� �����մϴ�.
        boss.RotateTowardsTarget();

        //  ���� ����: ���� �ð��� ������ �ٽ� �����Ϸ� ���ϴ�.
        if (stateTimer >= idleDuration)
        {
            boss.ReturnToDecision();
        }
    }
}