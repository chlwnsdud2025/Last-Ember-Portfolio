using UnityEngine;

public class Humanoid_Boss_Dodge_State : BossBaseState
{
    //  애니메이터 파라미터 및 상태 해시값 (미리 캐싱하여 성능 극대화)
    private readonly int dodgeXHash = Animator.StringToHash("DodgeX");
    private readonly int dodgeYHash = Animator.StringToHash("DodgeY");
    private readonly int dodgeStateHash = Animator.StringToHash("Dodge"); // 상태 이름 해시

    public Humanoid_Boss_Dodge_State(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        boss.animator.applyRootMotion = true;

        float distanceToPlayer = Vector3.Distance(boss.transform.position, boss.target.position);
        float targetX = 0f;
        float targetY = 0f;



        if (distanceToPlayer < 3f)
        {
            targetY = -1f; // 백스텝
        }
        else if (distanceToPlayer > 10f)
        {
            targetY = 1f;
        }
        else if (distanceToPlayer >= 3f && distanceToPlayer <= 10f)
        {
            targetX = Random.value > 0.5f ? 1f : -1f; // 사이드스텝
        }

        boss.animator.SetFloat(dodgeXHash, targetX);
        boss.animator.SetFloat(dodgeYHash, targetY);

        boss.animator.CrossFadeInFixedTime("Dodge", 0.1f);
    }

    public override void Update()
    {
        boss.RotateTowardsTarget();

        //  유저님의 아이디어 적용: 진행도를 가져와서 판단!
        AnimatorStateInfo stateInfo = boss.animator.GetCurrentAnimatorStateInfo(0);

        // 1. 현재 재생 중인 상태가 완벽하게 'Dodge'인지 확인 (트랜지션 꼬임 방지)
        // 2. 애니메이션 진행도가 95% 이상 진행되었는지 확인 (1.0으로 딱 맞추면 1프레임 튀는 현상 방지)
        if (stateInfo.shortNameHash == dodgeStateHash && stateInfo.normalizedTime >= 0.95f)
        {
            boss.ReturnToDecision();
        }
    }

    public override void Exit()
    {
        boss.animator.applyRootMotion = false;
    }
}