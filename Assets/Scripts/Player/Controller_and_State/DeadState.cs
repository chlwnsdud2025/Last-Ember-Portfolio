using UnityEngine;

public class DeadState : PlayerBaseState
{
  
    public DeadState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("상태: Dead");
        
    }

    public override void Update()
    {
        
    }

    // 대쉬 중에도 공격/대쉬 중복 입력 방지
    public override void OnAttackInput() { /* 무시 */ }
    public override void OnDashInput() { /* 무시 */ }
}
