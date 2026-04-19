using System;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    // ���߿� �� �Ӹ� �� ü�¹� UI�� ���� �� ����� �̺�Ʈ
    public event Action<float, float> OnEnemyHpChanged;

    private EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        currentHealth = maxHealth;
    }

    // �÷��̾��� ����(WeaponController)���� ���� ���� �� �� �Լ��� ȣ���ϰ� �˴ϴ�.
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // ü�¹� UI ������Ʈ ��ȣ �߼�
        OnEnemyHpChanged?.Invoke(currentHealth, maxHealth);

        // �������� �Ծ��� ���� ���� ��ȯ (Hit �Ǵ� Dead)
        if (enemyController != null)
        {
            if (currentHealth <= 0)
            {
                enemyController.stateMachine.ChangeState(enemyController.deadState);
            }
            else
            {
                // ü���� �����ִٸ� �ǰ�(����) ���·�
                enemyController.stateMachine.ChangeState(enemyController.hitsState);
            }
        }
    }
}
