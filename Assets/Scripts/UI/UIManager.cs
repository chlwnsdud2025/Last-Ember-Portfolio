using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    // ��𼭵� ���� ������ �̱���
    public static UIManager Instance { get; private set; }

    [Header("Player Reference")]
    [SerializeField] private PlayerStats playerStats;

    [Header("UI Components")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private GameObject gameOver;


    [Header("Size Settings")]
    [Tooltip("ü�� 1�� UI ���� �ȼ� ���� (��: 2�� �����ϸ� �ִ�ü�� 100�� �� ���� 200)")]
    [SerializeField] private float hpWidthMultiplier = 0.5f;
    [Tooltip("���׹̳� 1�� UI ���� �ȼ� ����")]
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
        // �÷��̾� ��ũ��Ʈ�� �̺�Ʈ�� �Լ� ���� (����)
        if (playerStats != null)
        {
            playerStats.OnHpChanged += UpdateHpUI;
            playerStats.OnStaminaChanged += UpdateStaminaUI;
        }
    }

    void OnDisable()
    {
        // �޸� ���� ������ ���� ���� ����
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
            // 1. �� ����
            hpSlider.maxValue = max;
            hpSlider.value = current;

            // 2. �ִ� ü�¿� ���� �����̴��� ���� ����(Width) ����
            RectTransform rect = hpSlider.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(max * hpWidthMultiplier, rect.sizeDelta.y);
        }
    }

    private void UpdateStaminaUI(float current, float max)
    {
        if (staminaSlider != null)
        {
            // 1. �� ����
            staminaSlider.maxValue = max;
            staminaSlider.value = current;

            // 2. �ִ� ���׹̳��� ���� �����̴��� ���� ����(Width) ����
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
