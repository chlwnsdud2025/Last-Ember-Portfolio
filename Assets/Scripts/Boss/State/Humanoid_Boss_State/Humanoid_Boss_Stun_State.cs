using UnityEngine;

public class Humanoid_Boss_Stun_State : BossBaseState
{
    // ������: �θ�(BossBaseState)���� ���� ��Ʈ�ѷ��� ���� �ӽ��� �Ѱ��ݴϴ�.
    public Humanoid_Boss_Stun_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("���� Stun ���� ����!");
        // ex) boss.animator.CrossFadeInFixedTime("Idle", 0.2f);
    }

    public override void Update()
    {
        // ex) �÷��̾���� �Ÿ��� �缭 �ȱ⳪ �ٱ�� ��ȯ�ϴ� ����
    }

    public override void Exit()
    {
        // ���¸� �������� �� �ʿ��� ���� �۾�
    }
}