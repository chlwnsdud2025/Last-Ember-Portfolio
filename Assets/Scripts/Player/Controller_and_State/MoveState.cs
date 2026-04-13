using UnityEngine;

public class MoveState : PlayerBaseState
{
    private int runAnimHash = Animator.StringToHash("Run");
    public MoveState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter() {
        Debug.Log("상태: Move");

        player.animator.CrossFadeInFixedTime(runAnimHash, 0.1f);

    }

    public override void Update()
    {
        if (player.moveInput == Vector2.zero)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        // 2. 전력질주(Sprint) 전환
        if (player.isSprintButtonHeld /* && player.stats.currentStamina > 0 */)
        {
            stateMachine.ChangeState(player.sprintState);
            return;
        }

        if (player.isWalkButtonHeld)
        {
            stateMachine.ChangeState(player.walkState);
            return;
        }

        // 3. [핵심] 락온 여부에 따른 애니메이터 파라미터 전달 (블렌딩 제어)
        if (player.isLockedOn)
        {
            if(runAnimHash == Animator.StringToHash("Run"))
            {
                runAnimHash = Animator.StringToHash("LockOnRun");
                player.animator.CrossFadeInFixedTime(runAnimHash, 0.1f);
            }
            
            // 락온 중: 8방향 블렌드 트리를 위해 X, Y 좌표를 그대로 전달
            // (0.1f는 댐핑 값으로, 방향을 바꿀 때 모션이 뚝뚝 끊기지 않게 부드럽게 이어줍니다)
            player.animator.SetFloat("InputX", player.moveInput.x, 0.1f, Time.deltaTime);
            player.animator.SetFloat("InputY", player.moveInput.y, 0.1f, Time.deltaTime);
        }
        

        HandleRotation();
        Vector3 camForward = player.cameraTransform.forward;
        Vector3 camRight = player.cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camRight * player.moveInput.x + camForward * player.moveInput.y;

        player.characterController.Move(moveDir * player.moveSpeed * Time.deltaTime);
    }




}

