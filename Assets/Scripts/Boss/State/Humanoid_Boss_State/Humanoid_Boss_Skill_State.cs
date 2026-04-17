using UnityEngine;

public class Humanoid_Boss_Skill_State : BossBaseState
{
    // 생성자: 부모(BossBaseState)에게 보스 컨트롤러와 상태 머신을 넘겨줍니다.
    public Humanoid_Boss_Skill_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("보스 Skill 상태 진입!");
        // ex) boss.animator.CrossFadeInFixedTime("Idle", 0.2f);
    }

    public override void Update()
    {
        // ex) 플레이어와의 거리를 재서 걷기나 뛰기로 전환하는 로직
    }

    public override void Exit()
    {
        // 상태를 빠져나갈 때 필요한 정리 작업
    }
}