using UnityEngine;

public class IdleState : PlayerBaseState
{
    private readonly int idleAnimHash = Animator.StringToHash("Idle");
    public IdleState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter() { 
        Debug.Log("상태: Idle");
        player.animator.CrossFade(idleAnimHash, 0.05f);
    }

    public override void Update()
    {
        // 이동 입력이 있으면 Move 상태로 전환
        if (player.moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }
}
