using UnityEngine;

public class Humanoid_Boss_Sleep_State : BossBaseState
{
    public Humanoid_Boss_Sleep_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        // 보스가 대기할 때 재생할 애니메이션 (예: 무릎 꿇고 있기, 앉아있기 등)
        // 따로 만든 게 없다면 기본 "Idle"을 넣으셔도 됩니다.
        boss.animator.CrossFadeInFixedTime("Idle", 0.1f);
    }

    public override void Update()
    {
        
    }

    public override void Exit()
    {
        // 잠에서 깨어날 때 포효를 하거나, 락온 포인트를 활성화하는 등
        // 전투 시작 전처리를 여기서 할 수 있습니다.
        Debug.Log("보스가 깨어났습니다!");
    }
}