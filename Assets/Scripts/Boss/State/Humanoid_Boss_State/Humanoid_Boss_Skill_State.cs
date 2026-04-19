using UnityEngine;

public class Humanoid_Boss_Skill_State : BossBaseState
{
    // ������: �θ�(BossBaseState)���� ���� ��Ʈ�ѷ��� ���� �ӽ��� �Ѱ��ݴϴ�.
    public Humanoid_Boss_Skill_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("���� Skill ���� ����!");
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