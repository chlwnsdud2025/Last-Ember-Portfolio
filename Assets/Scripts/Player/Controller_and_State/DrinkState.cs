using UnityEngine;

public class DrinkState : PlayerBaseState
{
    private float healTiming = 0.5f;
    private bool hasHealed = false;
    private bool isDrinkSuccessful = false; // 물약이 있어 마시기에 성공했는지 여부

    private readonly int walkAnimHash = Animator.StringToHash("Walk");
    private readonly int lockOnWalkAnimHash = Animator.StringToHash("LockOnWalk");
    private readonly int idleAnimHash = Animator.StringToHash("Idle");

    private int currentLowerStateHash;

    public DrinkState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        hasHealed = false;
        currentLowerStateHash = 0;

        // 1. 물약 개수 확인
        if (player.stats.currentPotions > 0)
        {
            // 성공: 물약 소모 및 Drink 애니메이션 재생
            isDrinkSuccessful = true;
            player.stats.ConsumePotion();
            player.animator.CrossFadeInFixedTime("Drink", 0.1f, 1);
            Debug.Log("물약 마시기 시작!");
        }
        else
        {
            // 실패: 물약 없음 알림 및 CantDrink 애니메이션 재생
            isDrinkSuccessful = false;
            player.animator.CrossFadeInFixedTime("DrinkFail", 0.1f, 1);
            Debug.Log("물약이 부족합니다! (실패 모션 재생)");
        }
    }

    public override void Update()
    {
        // 2. 현재 상체 레이어(1번) 정보 가져오기
        AnimatorStateInfo upperStateInfo = player.animator.GetCurrentAnimatorStateInfo(1);

        // 애니메이션이 아직 바뀌는 중이거나, 우리가 재생시킨 이름이 아니면 이동만 처리
        bool isPlayingAction = upperStateInfo.IsName("Drink") || upperStateInfo.IsName("DrinkFail");
        if (!isPlayingAction || player.animator.IsInTransition(1))
        {
            UpdateMovementAndRotation();
            return;
        }

        float progress = upperStateInfo.normalizedTime;

        // 3. 회복 처리 (성공했을 때만 특정 타이밍에 실행)
        if (isDrinkSuccessful && progress >= healTiming && !hasHealed)
        {
            hasHealed = true;
            player.stats.Heal();
        }

        // 하체 이동 및 회전은 공통 처리
        UpdateMovementAndRotation();

        // 4. 애니메이션 종료 체크
        if (progress >= 1.0f)
        {
            player.animator.CrossFadeInFixedTime("Empty", 0.1f, 1);
            stateMachine.ChangeState(player.idleState);
        }
    }

    private void UpdateMovementAndRotation()
    {
        HandleRotation();

        Vector3 cameraForward = player.cameraTarget.forward;
        Vector3 cameraRight = player.cameraTarget.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDir = (cameraForward * player.moveInput.y + cameraRight * player.moveInput.x).normalized;

        float drinkMoveSpeed = player.moveSpeed * 0.5f;
        player.MoveCharacter(moveDir * drinkMoveSpeed * Time.deltaTime);

        UpdateLowerBodyAnimation();
    }

    private void UpdateLowerBodyAnimation()
    {
        int targetHash;
        if (player.moveInput != Vector2.zero)
        {
            targetHash = player.isLockedOn ? lockOnWalkAnimHash : walkAnimHash;
            if (player.isLockedOn)
            {
                player.animator.SetFloat("InputX", player.moveInput.x, 0.1f, Time.deltaTime);
                player.animator.SetFloat("InputY", player.moveInput.y, 0.1f, Time.deltaTime);
            }
        }
        else
        {
            targetHash = idleAnimHash;
            player.animator.SetFloat("InputX", 0f, 0.1f, Time.deltaTime);
            player.animator.SetFloat("InputY", 0f, 0.1f, Time.deltaTime);
        }

        if (currentLowerStateHash != targetHash)
        {
            player.animator.CrossFadeInFixedTime(targetHash, 0.1f, 0);
            currentLowerStateHash = targetHash;
        }
    }

    public override void Exit()
    {
        // 어떤 이유로든 상태를 나갈 때 상체 레이어 초기화
        player.animator.CrossFadeInFixedTime("Empty", 0.1f, 1);
    }
}