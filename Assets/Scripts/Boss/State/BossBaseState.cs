using UnityEngine;

public abstract class BossBaseState
{
    protected BossBaseController boss;
    protected BossStateMachine stateMachine;

    // ������: �ڽ� ���µ��� ������ �� ������ ����� ���� �ӽ��� ����޽��ϴ�.
    public BossBaseState(BossBaseController boss, BossStateMachine stateMachine)
    {
        this.boss = boss;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }


}