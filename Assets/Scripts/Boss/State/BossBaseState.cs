using UnityEngine;

public abstract class BossBaseState
{
    protected BossBaseController boss;
    protected BossStateMachine stateMachine;

    // 생성자: 자식 상태들이 생성될 때 보스의 몸통과 상태 머신을 연결받습니다.
    public BossBaseState(BossBaseController boss, BossStateMachine stateMachine)
    {
        this.boss = boss;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}