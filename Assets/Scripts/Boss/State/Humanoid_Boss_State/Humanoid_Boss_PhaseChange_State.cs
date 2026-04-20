using UnityEngine;

public class Humanoid_Boss_PhaseChange_State : BossBaseState
{
    private float roarDuration = 3f; // 포효 애니메이션 길이에 맞춰 설정
    private float stateTimer;

    public Humanoid_Boss_PhaseChange_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        stateTimer = 0f;
        boss.animator.applyRootMotion = false;

        //  보스 무적 처리나 이펙트 재생 코드를 여기에 넣습니다.
        // boss.isInvincible = true; 
        //Instantiate(phase2AuraEffect, boss.transform.position, Quaternion.identity);

        // 2페이즈 돌입 포효 애니메이션 재생
        boss.animator.CrossFadeInFixedTime("Pase2", 0.2f);
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= roarDuration)
        {
            boss.ReturnToDecision();
        }
    }

    public override void Exit()
    {
        //  포효가 끝나고 나갈 때, 비로소 보스의 뇌를 2페이즈로 각성시킵니다!
        boss.currentPhase = 2;

        // 무적 해제 등
        // boss.isInvincible = false;
        Debug.Log(" 보스가 2페이즈 패턴을 시작합니다!");
    }
}