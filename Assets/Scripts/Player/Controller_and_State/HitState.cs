using UnityEngine;

public class HitState : PlayerBaseState
{
    private readonly int hitAnimHash = Animator.StringToHash("Hit");
    public HitState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }


    public override void Enter()
    {
        Debug.Log("Enemy 상태: Hit (경직!)");

        player.animator.CrossFadeInFixedTime(hitAnimHash, 0.1f, 0, 0f);

        player.animator.applyRootMotion = true;
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // 현재 실제로 "Hit" 모션이 재생 중인지 확인
        if (stateInfo.shortNameHash == hitAnimHash && !player.animator.IsInTransition(0))
        {
            // 경직이 풀릴 시
            if (stateInfo.normalizedTime >= 0.9f)
            {

                stateMachine.ChangeState(player.moveState);
            }
        }

        player.MoveCharacter(Vector3.zero);
    }

    public override void Exit()
    {
        player.animator.applyRootMotion = false;
    }
}
