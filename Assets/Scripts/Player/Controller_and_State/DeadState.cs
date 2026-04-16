using UnityEngine;

public class DeadState : PlayerBaseState
{
    private readonly int deadAnimHash = Animator.StringToHash("Die"); // 사망 애니메이션 이름 확인 필요
    public DeadState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {

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

        UIManager.Instance.GameOver();

    }

    public override void Update()
    {
        player.MoveCharacter(Vector3.zero);
    }

    public override void Exit()
    {
        UIManager.Instance.Respawn();

    }

    // 사망 상태에서는 모든 입력 무시
    public override void OnAttackInput() { /* 무시 */ }
    public override void OnDashInput() { /* 무시 */ }
}