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

    public virtual void OnAttackInput() { stateMachine.ChangeState(player.attackState); }
    public virtual void OnDashInput() { stateMachine.ChangeState(player.dashState); }

    // 기본 이동 회전 (Root 기준)
    protected virtual void HandleRotation()
    {
        if (player.isLockedOn && player.currentTarget != null)
        {
            Vector3 dirToTarget = player.currentTarget.position - player.transform.position;
            dirToTarget.y = 0f;

            if (dirToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, 15f * Time.deltaTime);
                // 어긋난 모델 껍데기를 서서히 정면으로 동기화
                player.model.transform.localRotation = Quaternion.Slerp(player.model.transform.localRotation, Quaternion.identity, 15f * Time.deltaTime);
            }
        }
        else if (player.moveInput != Vector2.zero)
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
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, 15f * Time.deltaTime);
                // 어긋난 모델 껍데기를 서서히 정면으로 동기화
                player.model.transform.localRotation = Quaternion.Slerp(player.model.transform.localRotation, Quaternion.identity, 15f * Time.deltaTime);
            }
        }
    }


}