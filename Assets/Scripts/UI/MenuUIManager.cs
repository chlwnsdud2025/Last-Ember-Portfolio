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

        // 시작할 때 메뉴창 숨기기
        menuPanel.SetActive(false);

        // 버튼들에 실행할 함수 연결
        inventoryButton.onClick.AddListener(OpenMainInventory);
        equipmentButton.onClick.AddListener(OpenEquipment);
        settingButton.onClick.AddListener(OpenSettings);
    }

    private void Update()
    {
        // Tab 키를 누르면 메뉴창 켜고 끄기
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        bool isActive = menuPanel.activeSelf;
        menuPanel.SetActive(!isActive);

        if (!isActive) // 메뉴를 켤 때
        {
            // 3D 게임이므로 메뉴를 고르려면 마우스 커서가 나타나야 합니다!
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else // 메뉴를 끌 때
        {
            // 메뉴를 끄면 열려있던 장비창이나 인벤토리창도 싹 다 닫아줍니다.
            CloseAllWindows();

            // 다시 게임 화면으로 돌아가므로 커서를 숨기고 잠급니다.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OpenEquipment()
    {
        // 장비창 열기!
        EquipmentUIManager.Instance.OpenWindow();

        // (선택) 장비창을 열었을 때 좌측 메뉴를 계속 띄워둘지, 숨길지 결정
        // menuPanel.SetActive(false); 
    }

    private void OpenMainInventory()
    {
        // 메인 인벤토리(가방) 열기! (나중에 구현할 예정)
        MainInventoryUIManager.Instance.OpenWindow();
    }

    private void OpenSettings()
    {
        // 설정창 열기! (제일 마지막에 구현할 예정)
        Debug.Log("설정 창 오픈 요청!");
    }

    public void CloseAllWindows()
    {
        // 1. 장비창 닫기
        if (EquipmentUIManager.Instance != null)
        {
            EquipmentUIManager.Instance.CloseWindow();
        }

        // 2. 장비 교체용 팝업 리스트 닫기
        if (EquipSelectionPopupUI.Instance != null)
        {
            EquipSelectionPopupUI.Instance.ClosePopup();
        }

        // 3. ★ 새로 추가: 메인 인벤토리 창 닫기!
        if (MainInventoryUIManager.Instance != null)
        {
            MainInventoryUIManager.Instance.CloseWindow();
        }
    }
}