using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] private float maxHp = 100f;
    private float currentHp;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    private float currentStamina;
    [SerializeField] private float staminaRegenRate = 15f;

    [Header("Regen Delay Settings")]
    [SerializeField] private float regenDelay = 1.5f; 
    private float lastUseTime;

    [Header("Potions")]
    public int maxPotions = 3;         // 최대 소지 가능 물약 수
    public int currentPotions = 3;     // 현재 남은 물약 수
    public float healAmount = 40f;     // 한 번 마실 때 차오르는 회복량

    public event Action<float, float> OnHpChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action<int> OnMoneyChanged;
    public event Action<int, int> OnPotionChanged;
    public bool canRegen = true;

    [Header("Currency")]
    public int currentMoney = 0;

    void Awake()
    {
        currentHp = maxHp;
        currentStamina = maxStamina;
    }

    void Start()
    {
        RefreshUI();
    }

    void Update()
    {
        if (Time.time > lastUseTime + regenDelay && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
    }

    public float CurrentStamina => currentStamina;

    public void RefreshUI()
    {
        OnHpChanged?.Invoke(currentHp, maxHp);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public bool TakeDamage(float amount)
    {
        currentHp = Mathf.Max(currentHp - amount, 0);
        OnHpChanged?.Invoke(currentHp, maxHp);

        return currentHp <= 0; 
    }

    public bool UseStamina(float amount)
    {
        if (amount > 0)
        {
            lastUseTime = Time.time;
        }

        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }

        // ������ ��� ó��
        if (currentStamina > 0)
        {
            currentStamina = 0;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        return false;
    }
    public void DelayRegen()
    {
        lastUseTime = Time.time;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        Debug.Log($"돈 획득! +{amount} (현재 잔액: {currentMoney})");

        // UI 업데이트 방송 송출
        OnMoneyChanged?.Invoke(currentMoney);
    }


    public void RefillPotions()
    {
        currentPotions = maxPotions;
        Debug.Log(" 화톳불: 물약이 모두 보충되었습니다.");
        OnPotionChanged?.Invoke(currentPotions, maxPotions);
    }

    public bool CanDrinkPotion()
    {
        return currentPotions > 0 && currentHp < maxHp;
    }

    public void ConsumePotion()
    {
        if (currentPotions > 0)
        {
            currentPotions--;
            OnPotionChanged?.Invoke(currentPotions, maxPotions);
        }
    }

    public void Heal()
    {
        currentHp += healAmount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        OnHpChanged?.Invoke(currentHp, maxHp); 
    }

}
