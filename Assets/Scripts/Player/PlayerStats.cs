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

    public event Action<float, float> OnHpChanged;
    public event Action<float, float> OnStaminaChanged;

    public bool canRegen = true;

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

}
