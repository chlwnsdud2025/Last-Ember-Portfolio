using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class MainInventoryUIManager : MonoBehaviour
{
    public static MainInventoryUIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject inventoryWindow; // 인벤토리 창 전체 패널
    public Transform scrollContent;    // 아이템이 뜰 Grid Layout Group 영역
    public GameObject inventorySlotPrefab;

    [Header("Item Details")]
    public Image detailIconImage; //  추가: 툴팁에 띄울 아이콘 이미지
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailDescText;
    public TextMeshProUGUI detailStatsText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (inventoryWindow != null) inventoryWindow.SetActive(false);
        ClearItemDetails();
    }

    // ==========================================
    // 1. 창 열기 / 닫기
    // ==========================================
    public void OpenWindow()
    {
        UIManager.Instance.OpenPanel(inventoryWindow);
        ShowItemsByCategory(null);
    }

    public void CloseWindow()
    {
        UIManager.Instance.ClosePanel(inventoryWindow);
    }

    // ==========================================
    // 2. 탭(Tab) 버튼 기능: 카테고리별 아이템 띄우기
    // ==========================================
    // 유니티 버튼의 OnClick()에 연결할 함수들입니다.
    public void OnClickAllTab() { ShowItemsByCategory(null); }
    public void OnClickWeaponTab() { ShowItemsByCategory(ItemType.Weapon); }
    public void OnClickArmorTab() { ShowItemsByCategory(ItemType.Armor); }
    public void OnClickArtifactTab() { ShowItemsByCategory(ItemType.Artifact); }

    public void OnClickConsumableTab() { ShowItemsByCategory(ItemType.Consumable); }

    // 실제 화면에 아이템을 그리는 핵심 로직
    private void ShowItemsByCategory(ItemType? categoryFilter)
    {
        // 1. 기존 슬롯 싹 다 지우기
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        // 2. 데이터 가져오기
        List<ItemInstance> itemsToShow = InventoryManager.Instance.allItems;

        // 필터가 null이 아니면 해당 카테고리만 걸러냅니다.
        if (categoryFilter.HasValue)
        {
            itemsToShow = InventoryManager.Instance.GetItemsByType(categoryFilter.Value);
        }

        // 3. 슬롯 생성
        foreach (ItemInstance item in itemsToShow)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, scrollContent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

            if (slotUI != null)
            {
                // 메인 인벤토리에서는 아이템을 클릭하면 '상세 정보'를 띄우도록 Action을 넘겨줍니다!
                slotUI.InitSlot(item, ShowItemDetails);
            }
        }
    }

    // ==========================================
    // 3. 아이템 클릭 시 상세 정보 띄우기
    // ==========================================
    private void ShowItemDetails(ItemInstance item)
    {
        if (detailIconImage != null && item.itemData.icon != null)
        {
            detailIconImage.sprite = item.itemData.icon;
            detailIconImage.enabled = true; // 아이콘 보이게 켜기
        }

        detailNameText.text = item.itemData.itemName;
        detailDescText.text = item.itemData.description;

        if (item.itemData is WeaponData weapon)
        {
            detailStatsText.text = $"STR: {weapon.baseDamage} (Count: {item.amount})";
        }
        else
        {
            detailStatsText.text = $"Count: {item.amount}";
        }
    }

    private void ClearItemDetails()
    {
        detailNameText.text = "";
        detailDescText.text = "Select Item.";
        detailStatsText.text = "";
    }
}