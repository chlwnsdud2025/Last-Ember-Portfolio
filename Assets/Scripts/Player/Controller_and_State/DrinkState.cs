using UnityEngine;

public class DrinkState : PlayerBaseState
{
    private float healTiming = 0.5f; // 애니메이션이 50% 진행됐을 때 피가 참
    private bool hasHealed = false;

    public DrinkState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        hasHealed = false;

        // 1. 상태 진입 즉시 물약 개수부터 깝니다! (맞아서 캔슬돼도 물약은 날아감)
        player.stats.ConsumePotion();

        // 2. 이동/회전 완전 봉인 (혹은 느리게 걷기 허용)
        player.animator.applyRootMotion = false;
        player.currentRollDir = Vector3.zero;

        // 3. 마시는 애니메이션 재생 (Animator에 "Drink" 노드 필요)
        player.animator.CrossFadeInFixedTime("Drink", 0.1f);
        Debug.Log(" 물약 마시기 시작...");
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("Drink") || player.animator.IsInTransition(0)) return;

        float progress = stateInfo.normalizedTime;

        // 핵심: 지정된 타이밍(예: 0.5)에 도달했을 때 딱 한 번 회복!
        if (progress >= healTiming && !hasHealed)
        {
            hasHealed = true;
            player.stats.Heal();
            // 여기에 초록색 회복 파티클이나 사운드를 재생하면 완벽합니다!
        }

        // 애니메이션이 끝나면 대기 상태로 복귀
        if (progress >= 1.0f)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        // 마시다 맞아서 HitState로 강제 전환될 경우를 대비
        if (!hasHealed)
        {
            Debug.Log("캔슬되었습니다");
        }
    }
}