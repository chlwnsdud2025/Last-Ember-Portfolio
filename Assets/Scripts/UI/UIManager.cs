using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    // 어디서든 접근 가능한 싱글톤
    public static UIManager Instance { get; private set; }

    [Header("Player Reference")]
    [SerializeField] private PlayerStats playerStats;

    [Header("UI Components")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private GameObject gameOver;


    [Header("Size Settings")]
    [Tooltip("체력 1당 UI 바의 픽셀 길이 (예: 2로 설정하면 최대체력 100일 때 길이 200)")]
    [SerializeField] private float hpWidthMultiplier = 0.5f;
    [Tooltip("스테미나 1당 UI 바의 픽셀 길이")]
    [SerializeField] private float staminaWidthMultiplier = 0.3f;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        gameOver.SetActive(false);
    }

    void OnEnable()
    {
        // 플레이어 스크립트의 이벤트에 함수 연결 (구독)
        if (playerStats != null)
        {
            playerStats.OnHpChanged += UpdateHpUI;
            playerStats.OnStaminaChanged += UpdateStaminaUI;
        }
    }

    void OnDisable()
    {
        // 메모리 누수 방지를 위한 구독 해제
        if (playerStats != null)
        {
            playerStats.OnHpChanged -= UpdateHpUI;
            playerStats.OnStaminaChanged -= UpdateStaminaUI;
        }
    }

    private void UpdateHpUI(float current, float max)
    {
        if (hpSlider != null)
        {
            // 1. 값 갱신
            hpSlider.maxValue = max;
            hpSlider.value = current;

            // 2. 최대 체력에 맞춰 슬라이더의 가로 길이(Width) 조절
            RectTransform rect = hpSlider.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(max * hpWidthMultiplier, rect.sizeDelta.y);
        }
    }

    private void UpdateStaminaUI(float current, float max)
    {
        if (staminaSlider != null)
        {
            // 1. 값 갱신
            staminaSlider.maxValue = max;
            staminaSlider.value = current;

            // 2. 최대 스테미나에 맞춰 슬라이더의 가로 길이(Width) 조절
            RectTransform rect = staminaSlider.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(max * staminaWidthMultiplier, rect.sizeDelta.y);
        }
    }

    public  void GameOver()
    {
        gameOver.SetActive(true);
    }

    public  void Respawn()
    {
        gameOver.SetActive(false);

    }

}
