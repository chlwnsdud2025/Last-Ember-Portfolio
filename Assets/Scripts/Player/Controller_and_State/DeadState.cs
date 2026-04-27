using UnityEngine;

public class DeadState : PlayerBaseState
{
    private readonly int deadAnimHash = Animator.StringToHash("Die");

    //  타이머용 변수 추가
    private float deathTimer = 0f;
    private float respawnDelay = 4.0f; // 4초 뒤 부활 (YOU DIED 연출 시간에 맞춰 조절하세요)
    private bool hasRespawned = false;

    public DeadState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("플레이어 사망!");

        // 타이머 초기화
        deathTimer = 0f;
        hasRespawned = false;

        // 사망 애니메이션 재생
        player.animator.CrossFadeInFixedTime(deadAnimHash, 0.2f, 0, 0f);

        // 죽었을 때 움직이지 않도록 처리
        player.animator.applyRootMotion = true;

        // 타겟팅 해제
        if (player.isLockedOn)
        {
            player.ClearLockOn();
        }

        // 무기 콜라이더 끄기
        if (player.currentWeaponScript != null)
        {
            player.currentWeaponScript.DisableHitbox();
        }

        // 다잉 메세지 띄우기
        UIManager.Instance.GameOver();
    }

    public override void Update()
    {
        player.MoveCharacter(Vector3.zero);

        //  4초 뒤 자동 부활 로직 실행
        deathTimer += Time.deltaTime;
        if (deathTimer >= respawnDelay && !hasRespawned)
        {
            hasRespawned = true;
            player.RespawnPlayer(); // 상태를 Idle로 강제 변경하며 위치 이동
        }
    }

    public override void Exit()
    {
        player.animator.applyRootMotion = false;
        UIManager.Instance.Respawn();
    }

    // 사망 상태에서는 모든 입력 무시
    public override void OnAttackInput() { /* 무시 */ }
    public override void OnDashInput() { /* 무시 */ }
}