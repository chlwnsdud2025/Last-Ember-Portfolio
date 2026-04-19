using System;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    // 체력 변경 시 UI 등에 알림을 보내는 이벤트 (현재 체력, 최대 체력 전달)
    //public event Action<float, float> OnEnemyHpChanged;

    private EnemyController enemyController;
    private BossBaseController bossController;


    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        bossController = GetComponent<BossBaseController>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);// 체력이 0 미만이나 최대치 초과가 되지 않게 방지

        //OnEnemyHpChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"남은 체력:{currentHealth}");
        
        if (currentHealth <= 0)
        {
            enemyController.stateMachine.ChangeState(enemyController.deadState);
            bossController.stateMachine.ChangeState(bossController.deadState);
        }
        else
        {
            enemyController.stateMachine.ChangeState(enemyController.hitsState);
        }
        
    }
}
