using UnityEngine;

public class RestState : PlayerBaseState
{
    public RestState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.animator.SetBool("isResting", true);
        player.animator.CrossFadeInFixedTime("Bonfire.Start", 0.1f, 0);

        // ★ 수정: 개별 매니저가 아닌 공통 UI 매니저를 켬
        if (BonfireUIManager.Instance != null)
            BonfireUIManager.Instance.ToggleRestingHUD(true);
    }

    public override void Update()
    {
        // 중력 유지용 고정
        player.MoveCharacter(Vector3.zero);

        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // 일어나는 애니메이션(End) 종료 체크
        if (stateInfo.IsName("Bonfire.End") && stateInfo.normalizedTime >= 0.85f)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        player.animator.SetBool("isResting", false);

        // 공통 UI 매니저를 끔
        if (BonfireUIManager.Instance != null)
            BonfireUIManager.Instance.ToggleRestingHUD(false);
    }

    public override void OnInteractInput()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // 완전히 앉아서 '쉬는 상태(Bonfire.Idle)'일 때만 상호작용 키(E)를 받아서 일어남
        if (stateInfo.IsName("Bonfire.Idle"))
        {
            player.animator.CrossFadeInFixedTime("Bonfire.End", 0.1f, 0);
            player.animator.SetBool("isResting", false);
        }
    }

    // 다른 액션들은 무시
    public override void OnAttackInput() { /* 무시 */ }
    public override void OnDashInput() { /* 무시 */ }
    public override void OnPotionInput() { /* 무시 */ }
}