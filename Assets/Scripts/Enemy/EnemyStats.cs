using System;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float deadCoin = 0;
    //  방송국(Event) 개설: "체력이 변했다!", "피가 0이 됐다!"
    public event Action<float, float> OnHpChanged;
    public event Action OnZeroHealth;
    public event Action OnTakeDamage;


    [Header("Reward")]
    public int dropMoneyAmount = 50;
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return; // 이미 죽었으면 무시

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHpChanged?.Invoke(currentHealth, maxHealth);
        OnTakeDamage?.Invoke();

        //  체력이 0이 되어 죽는 바로 그 순간!
        if (currentHealth <= 0)
        {
            // 1. 맵에 있는 플레이어 지갑을 찾아서 돈을 넣어줍니다.
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddMoney(dropMoneyAmount);
            }

            // 2. 죽음 방송 송출 (상태 머신이 이거 듣고 DeadState로 감)
            OnZeroHealth?.Invoke();
        }
    }



}