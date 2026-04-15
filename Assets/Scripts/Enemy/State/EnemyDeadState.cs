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
            if (player.currentTarget == enemy.transform)
            {
                player.ClearLockOn(); // 락온 해제
            }
        }

        if (enemy.currentWeaponScript != null)
        {
            enemy.currentWeaponScript.DisableHitbox();
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
