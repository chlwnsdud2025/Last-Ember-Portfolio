using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{

    private readonly int deadAnimHash = Animator.StringToHash("Die");

    public EnemyDeadState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        enemy.animator.applyRootMotion = true;
        enemy.animator.CrossFadeInFixedTime(deadAnimHash, 0.2f, 0, 0f);

        PlayerController player = Object.FindObjectOfType<PlayerController>();

        if (player != null && player.isLockedOn)
        {
            // [핵심 수정] 타겟이 '몸통'이거나 '락온 포인트(가슴)'일 때 모두 락온을 풉니다!
            if (player.currentTarget == enemy.transform || player.currentTarget == enemy.lockOnPoint)
            {
                player.ClearLockOn(); // 락온 해제
            }
        }

        if (enemy.weapons != null)
        {
            foreach (EnemyWeaponController weapon in enemy.weapons)
            {
                if (weapon != null) weapon.DisableHitbox();
            }
        }

        // 콜라이더를 끄기
        if (enemy.characterController != null)
        {
            enemy.characterController.enabled = false;
        }

        
    }

    

    public override void Update()
    {
    }

    public override void Exit()
    {
    }

}
