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
    [SerializeField] private float regenDelay = 1.5f; // 회복이 시작되기 전 대기 시간
    private float lastUseTime; // 마지막으로 스테미나를 사용한 시간

    // UI Manager가 구독할 이벤트 (현재값, 최대값 전달)
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
        // 시작 시 현재 수치를 UI에 한 번 전달
        RefreshUI();
    }

    void Update()
    {
        // canRegen이 true일 때만 자동 회복
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

    // 데미지 입을 때 호출 (FSM 피격 상태 등에서)
    public bool TakeDamage(float amount)
    {
        currentHp = Mathf.Max(currentHp - amount, 0);
        OnHpChanged?.Invoke(currentHp, maxHp);

        return currentHp <= 0; // 죽었으면 true, 살았으면 false
    }

    // 스테미나 소모 (FSM 공격/대시 상태에서 호출)
    public bool UseStamina(float amount)
    {
        // 스테미나를 소모하려는 시도가 있으면(양수일 때) 무조건 지연 타이머 초기화
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

        // 부족할 경우 처리
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
