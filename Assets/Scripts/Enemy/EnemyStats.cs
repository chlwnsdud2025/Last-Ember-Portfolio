using System;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    //  방송국(Event) 개설: "체력이 변했다!", "피가 0이 됐다!"
    public event Action<float, float> OnHpChanged;
    public event Action OnZeroHealth;
    public event Action OnTakeDamage;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return; // 이미 죽었으면 무시

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[EnemyStats] 데미지 {damage} 받음! 남은 체력: {currentHealth}");

        
        OnHpChanged?.Invoke(currentHealth, maxHealth);
        OnTakeDamage?.Invoke();

        if (currentHealth <= 0)
        {
            OnZeroHealth?.Invoke();
        }
    }
}