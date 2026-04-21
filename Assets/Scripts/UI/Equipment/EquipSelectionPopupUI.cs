using System.Collections.Generic;
using UnityEngine;

public class EquipSelectionPopupUI : MonoBehaviour
{
    public static EquipSelectionPopupUI Instance { get; private set; }

    public GameObject popupPanel;      // 팝업 패널 전체
    public Transform scrollContent;    // Scroll View의 Content 영역
    public GameObject inventorySlotPrefab; // 위에서 만든 InventorySlotUI가 붙은 프리팹

    private void Awake()
    {
        if (Instance == null) Instance = this;
        popupPanel.SetActive(false);
    }

    public void OpenPopup(ItemType targetType)
    {
        popupPanel.SetActive(true);

        // 기존 슬롯 청소
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        // 카테고리에 맞는 아이템만 쏙쏙 뽑아오기
        List<ItemInstance> filteredItems = InventoryManager.Instance.GetItemsByType(targetType);

        // 슬롯 생성
        foreach (ItemInstance item in filteredItems)
        {
            GameObject newSlotObj = Instantiate(inventorySlotPrefab, scrollContent);
            InventorySlotUI slotUI = newSlotObj.GetComponent<InventorySlotUI>();
            if (slotUI != null) slotUI.InitSlot(item, (selectedItem) =>
            {
                EquipmentManager.Instance.Equip(selectedItem);
                ClosePopup();
            });
        }
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}