using System;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    // 나중에 몹 머리 위 체력바 UI를 만들 때 사용할 이벤트
    public event Action<float, float> OnEnemyHpChanged;

    private EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        currentHealth = maxHealth;
    }

    // 플레이어의 무기(WeaponController)에서 적을 때릴 때 이 함수를 호출하게 됩니다.
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 체력바 UI 업데이트 신호 발송
        OnEnemyHpChanged?.Invoke(currentHealth, maxHealth);

        // 데미지를 입었을 때의 상태 전환 (Hit 또는 Dead)
        if (enemyController != null)
        {
            if (currentHealth <= 0)
            {
                enemyController.stateMachine.ChangeState(enemyController.deadState);
            }
            else
            {
                // 체력이 남아있다면 피격(경직) 상태로
                enemyController.stateMachine.ChangeState(enemyController.hitsState);
            }
        }
    }
}
