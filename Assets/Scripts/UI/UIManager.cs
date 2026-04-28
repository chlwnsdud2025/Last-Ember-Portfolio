using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player Reference")]
    [SerializeField] private PlayerStats playerStats;

    [Header("UI Components")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private GameObject gameOver;


    [Header("Size Settings")]
    [SerializeField] private float hpWidthMultiplier = 0.5f;
    [SerializeField] private float staminaWidthMultiplier = 0.3f;


    public GameObject gameClearPanel;

    private List<GameObject> activePanels = new List<GameObject>();//UI List

    public bool IsAnyUIOpen 
    {
        get
        {
            
            bool isTabMenuOpen = MenuUIManager.Instance != null && MenuUIManager.Instance.menuPanel.activeSelf;
            bool isBonfireOpen = BonfireUIManager.Instance != null && BonfireUIManager.Instance.restingMenuCanvas.activeSelf;
            return activePanels.Count > 0 || isTabMenuOpen || isBonfireOpen;
        }
    }

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
        if (playerStats != null)
        {
            playerStats.OnHpChanged += UpdateHpUI;
            playerStats.OnStaminaChanged += UpdateStaminaUI;
        }
    }

    void OnDisable()
    {
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
            hpSlider.maxValue = max;
            hpSlider.value = current;
            RectTransform rect = hpSlider.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(max * hpWidthMultiplier, rect.sizeDelta.y);
        }
    }

    private void UpdateStaminaUI(float current, float max)
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = max;
            staminaSlider.value = current;

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

    public void OpenPanel(GameObject panel)
    {
        if (panel == null) return;

        panel.SetActive(true);
        if (!activePanels.Contains(panel))
        {
            activePanels.Add(panel);
        }
        UpdateCursorState();
    }

    
    public void ClosePanel(GameObject panel)
    {
        if (panel == null) return;

        panel.SetActive(false);
        activePanels.Remove(panel);
        UpdateCursorState();
    }

    // ESC
    public void CloseTopPanel()
    {
        // 1. 열려있는 세부 패널(레벨업 창 등)이 있다면 그것부터 닫음
        if (activePanels.Count > 0)
        {
            GameObject topPanel = activePanels[activePanels.Count - 1];
            ClosePanel(topPanel);
        }
        // 2. 세부 패널이 하나도 없는데 화톳불 메뉴가 켜져있다면 화톳불 종료
        else if (BonfireUIManager.Instance != null && BonfireUIManager.Instance.restingMenuCanvas.activeSelf)
        {
            BonfireUIManager.Instance.OnClickLeave();

        }

        UpdateCursorState();
    }

    public void UpdateCursorState()
    {
        if (IsAnyUIOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    public void GameClear()
    {
        gameClearPanel.SetActive(true);
    }

    public void GameRestart()
    {
        SceneManager.LoadScene(1);
    }
    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 2. 실제 빌드된 파일(.exe, .apk 등)에서 게임을 종료하는 코드
        Application.Quit();
#endif
    }
}
