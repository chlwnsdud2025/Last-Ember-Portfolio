using UnityEngine;

public class DeadState : PlayerBaseState
{
    private readonly int deadAnimHash = Animator.StringToHash("Die"); // 사망 애니메이션 이름 확인 필요
    bool isdeadreal = false;
    public DeadState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        if (isdeadreal) { return; }

        isdeadreal = true;
        Debug.Log("플레이어 사망!");

        // 사망 애니메이션 재생
        player.animator.CrossFadeInFixedTime(deadAnimHash, 0.2f, 0, 0f);

        // 죽었을 때 움직이지 않도록 처리
        player.animator.applyRootMotion = true;

        // 타겟팅 해제
        if (player.isLockedOn)
        {
            player.ClearLockOn();
        }

        // 무기 콜라이더 끄기 (공격 중에 죽었을 경우 대비)
        if (player.currentWeaponScript != null)
        {
            player.currentWeaponScript.DisableHitbox();
        }
    }

    public override void Update()
    {
        // 사망 상태에서는 아무 행동도 하지 않음 (중력만 적용하려면 MoveCharacter(Vector3.zero) 정도만 호출)
        player.MoveCharacter(Vector3.zero);
    }

    public override void Exit()
    {
        // 부활 시스템이 있다면 여기서 초기화
    }

    // 사망 상태에서는 모든 입력 무시
    public override void OnAttackInput() { /* 무시 */ }
    public override void OnDashInput() { /* 무시 */ }
}