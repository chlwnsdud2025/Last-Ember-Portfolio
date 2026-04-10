using Unity.VisualScripting;
using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerController player;
    protected StateMachine stateMachine;

    public PlayerBaseState(PlayerController player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

    public virtual void OnAttackInput()
    {
        stateMachine.ChangeState(player.attackState);
    }

    public virtual void OnDashInput()
    {
        stateMachine.ChangeState(player.dashState);
    }



    //  부모의 회전 함수를 3D 액션(카메라 기준)에 맞게 덮어씁니다.
    protected virtual void HandleRotation()
    {
        if (player.moveInput != Vector2.zero)
        {
            Vector3 camForward = player.cameraTransform.forward;
            Vector3 camRight = player.cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camRight * player.moveInput.x + camForward * player.moveInput.y).normalized;

            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                // [수정] 이동할 때도 모델이 아닌 루트 자체를 회전시킵니다.
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, 15f * Time.deltaTime);
                player.model.transform.localRotation = Quaternion.identity;
            }
        }
    }


}