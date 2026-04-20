using UnityEngine;

public class Humanoid_Boss_Dead_State : BossBaseState
{
    private float deathTimer = 0f;
    private float timeToDisappear = 5f; // 죽고 나서 시체가 사라지거나 승리 UI가 뜰 때까지의 시간
    private bool hasTriggeredVictory = false;

    public Humanoid_Boss_Dead_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        Debug.Log(" 보스 사망 상태 진입!");
        deathTimer = 0f;
        hasTriggeredVictory = false;

        // 1. 움직임 완전 봉인 (루트 모션 및 중력 연산 차단)
        boss.animator.applyRootMotion = false;

        // 2. 충돌체 끄기 (플레이어가 시체에 걸려 못 지나가는 현상 방지)
        CharacterController cc = boss.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Collider col = boss.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3. 락온 포인트 파괴/비활성화 (플레이어의 락온 카메라가 시체에 계속 고정되는 버그 방지!)
        if (boss.lockOnPoint != null)
        {
            boss.lockOnPoint.gameObject.SetActive(false);
        }

        // 4. 사망 애니메이션 재생 (Animator에 "Die" 노드가 있어야 합니다)
        boss.animator.CrossFadeInFixedTime("Dead", 0.2f);
    }

    public override void Update()
    {
        //  죽었으므로 쳐다보기(Rotate)나 판단(Decision)은 절대 하지 않습니다.

        // 사망 후 연출 타이머 (시체가 서서히 녹아내리거나, 소울 흡수 연출 등)
        deathTimer += Time.deltaTime;

        if (deathTimer >= timeToDisappear && !hasTriggeredVictory)
        {
            hasTriggeredVictory = true;

            Debug.Log(" YOU DEFEATED 연출 시작!");
            // 여기에 게임 클리어 UI를 띄우거나, 경험치/아이템 드랍 코드를 넣으면 완벽합니다.

            // (선택) 보스 시체 비활성화
            // boss.gameObject.SetActive(false); 
        }
    }

    public override void Exit()
    {
        // 부활하는 3페이즈 기믹이 없는 이상, 이 함수는 불릴 일이 없습니다.
    }
}