using UnityEngine;

public class PickUpState : PlayerBaseState
{
    private readonly int pickupAnimHash = Animator.StringToHash("PickUp"); // 줍기 애니메이션 이름
    private float pickupTiming = 0.5f; // 손이 바닥에 닿는 타이밍 (0.0 ~ 1.0 비율)
    private bool hasPickedUp = false;

    public PickUpState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        hasPickedUp = false;

        // 줍기 모션 실행
        player.animator.CrossFadeInFixedTime(pickupAnimHash, 0.1f, 0);

        //Debug.Log("상태: Pickup");
    }

    public override void Update()
    {
        // 줍는 동안 제자리 고정 (중력 적용)
        player.MoveCharacter(Vector3.zero);

        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // 현재 애니메이션이 Pickup이 아니거나 전환 중이면 대기
        if (stateInfo.shortNameHash != pickupAnimHash || player.animator.IsInTransition(0)) return;

        float progress = stateInfo.normalizedTime;

        // 1. 손이 바닥에 닿는 정확한 타이밍에 획득 로직 실행
        if (progress >= pickupTiming && !hasPickedUp)
        {
            hasPickedUp = true;

            if (player.currentTargetItem != null)
            {
                player.currentTargetItem.ExecutePickUp(); // 아이템 획득 및 알림창 호출
                player.currentTargetItem = null;
            }
        }

        // 2. 애니메이션이 85% 이상 끝나면 대기 상태로 복귀
        if (progress >= 0.85f)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        player.currentTargetItem = null;
    }

    // 줍는 도중 캔슬 방지 (필요 시 OnDashInput은 풀어주어 회피 캔슬을 구현할 수도 있습니다)
    public override void OnAttackInput() { /* 무시 */ }
    public override void OnDashInput() { /* 무시 */ }
    public override void OnPotionInput() { /* 무시 */ }
}