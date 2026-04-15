using UnityEngine;

public abstract class BossBaseState
{
    protected BossController boss; // 모든 보스의 부모 타입
    protected BossStateMachine bossStateMachine;

    public BossBaseState(BossController boss, BossStateMachine stateMachine)
    {
        this.boss = boss;
        this.bossStateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

    // 기본 피격 로직 (상태별로 오버라이드 가능)
    public virtual void OnTakeDamage(float damage)
    {
        boss.currentHealth -= damage;
        // 기본적으로는 경직 없이 체력만 감소 (슈퍼 아머)
    }
}