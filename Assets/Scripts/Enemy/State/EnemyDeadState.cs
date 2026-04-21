using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{

    private readonly int deadAnimHash = Animator.StringToHash("Die");

    public EnemyDeadState(EnemyController enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }

    public override void Enter()
    {
        enemy.animator.applyRootMotion = true;
        enemy.animator.CrossFadeInFixedTime(deadAnimHash, 0.2f, 0, 0f);

        PlayerController player = Object.FindAnyObjectByType<PlayerController>();

        if (player != null && player.isLockedOn)
        {
            // [�ٽ� ����] Ÿ���� '����'�̰ų� '��� ����Ʈ(����)'�� �� ��� ����� Ǳ�ϴ�!
            if (player.currentTarget == enemy.transform || player.currentTarget == enemy.lockOnPoint)
            {
                player.ClearLockOn(); // ��� ����
            }
        }

        if (enemy.weapons != null)
        {
            foreach (EnemyWeaponController weapon in enemy.weapons)
            {
                if (weapon != null) weapon.DisableHitbox();
            }
        }

        // �ݶ��̴��� ���
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
