using UnityEngine;

public class HitState : PlayerBaseState
{
    private readonly int hitAnimHash = Animator.StringToHash("Hit");
    public HitState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }


    public override void Enter()
    {
        //Debug.Log("Enemy ����: Hit (����!)");

        player.animator.CrossFadeInFixedTime(hitAnimHash, 0.1f, 0, 0f);

        player.animator.applyRootMotion = true;
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // ���� ������ "Hit" ����� ��� ������ Ȯ��
        if (stateInfo.shortNameHash == hitAnimHash && !player.animator.IsInTransition(0))
        {
            // ������ Ǯ�� ��
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

    public override void OnAttackInput() {/*����*/}
    public override void OnDashInput() {/*����*/}
}
