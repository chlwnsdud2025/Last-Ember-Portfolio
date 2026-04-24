using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager Instance { get; private set; }

    [Header("Menu Panel")]
    public GameObject menuPanel; // Tab을 눌렀을 때 켜질 좌측 메뉴 UI 패널

    [Header("Menu Buttons")]
    public Button inventoryButton;
    public Button equipmentButton;
    public Button settingButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // 시작할 때 메뉴창 숨기기 (UIManager를 거치지 않고 초기화만 수행)
        menuPanel.SetActive(false);

        // 버튼들에 실행할 함수 연결
        inventoryButton.onClick.AddListener(OpenMainInventory);
        equipmentButton.onClick.AddListener(OpenEquipment);
        settingButton.onClick.AddListener(OpenSettings);
    }

    private void Update()
    {
        // Tab 키 입력 처리 (ActionMap을 사용 중이라면 그쪽으로 옮기는 것이 좋습니다)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        bool isActive = menuPanel.activeSelf;

        if (!isActive) // 메뉴를 켤 때
        {
            // ★ 수정: UIManager의 스택에 추가하여 켭니다.
            UIManager.Instance.OpenPanel(menuPanel);
        }
        else // 메뉴를 끌 때
        {
            // 메뉴를 끄면 열려있던 모든 하위 창들을 먼저 닫고
            CloseAllWindows();
            // ★ 수정: UIManager를 통해 메뉴판 자체도 닫습니다.
            UIManager.Instance.ClosePanel(menuPanel);
        }
    }

    private void OpenEquipment()
    {
        // 장비창 열기 (내부에서 UIManager.Instance.OpenPanel 호출)
        EquipmentUIManager.Instance.OpenWindow();
    }

    private void OpenMainInventory()
    {
        // 메인 인벤토리 열기 (내부에서 UIManager.Instance.OpenPanel 호출)
        MainInventoryUIManager.Instance.OpenWindow();
    }

    private void OpenSettings()
    {
        Debug.Log("설정 창 오픈 요청!");
        // 나중에 설정창도 UIManager.Instance.OpenPanel(settingsPanel) 방식으로 만드시면 됩니다.
    }

    // ★ 매우 중요: 모든 UI를 닫는 로직
    public void CloseAllWindows()
    {
        // 1. 장비 선택 팝업 리스트부터 닫기 (가장 위에 있을 확률이 높음)
        if (EquipSelectionPopupUI.Instance != null)
            EquipSelectionPopupUI.Instance.ClosePopup();

        // 2. 장비창 닫기
        if (EquipmentUIManager.Instance != null)
            EquipmentUIManager.Instance.CloseWindow();

        // 3. 메인 인벤토리 창 닫기
        if (MainInventoryUIManager.Instance != null)
            MainInventoryUIManager.Instance.CloseWindow();

        // ※ 각 Close 함수들은 내부에서 UIManager.Instance.ClosePanel()을 호출해야 합니다.
    }
}