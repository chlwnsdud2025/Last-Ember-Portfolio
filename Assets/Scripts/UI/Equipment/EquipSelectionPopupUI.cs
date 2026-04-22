using System.Collections.Generic;
using UnityEngine;

public class EquipSelectionPopupUI : MonoBehaviour
{
    public static EquipSelectionPopupUI Instance { get; private set; }

    public GameObject popupPanel;
    public Transform scrollContent;
    public GameObject inventorySlotPrefab;

    // ★ 추가: 현재 열려있는 팝업이 '몇 번 슬롯'을 위한 것인지 기억하는 변수
    private int currentTargetSlotIndex;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        popupPanel.SetActive(false);
    }

    // ★ 수정: 매개변수로 slotIndex를 추가로 받습니다.
    public void OpenPopup(ItemType targetType, int slotIndex)
    {
        currentTargetSlotIndex = slotIndex; // 몇 번 슬롯인지 기억!
        popupPanel.SetActive(true);

        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        List<ItemInstance> filteredItems = InventoryManager.Instance.GetItemsByType(targetType);

        foreach (ItemInstance item in filteredItems)
        {
            GameObject newSlotObj = Instantiate(inventorySlotPrefab, scrollContent);
            InventorySlotUI slotUI = newSlotObj.GetComponent<InventorySlotUI>();

            if (slotUI != null)
            {
                slotUI.InitSlot(item, (selectedItem) =>
                {
                    // ★ 수정: 장착할 때 "내가 기억해둔 슬롯 인덱스"를 같이 넘겨줍니다!
                    EquipmentManager.Instance.Equip(selectedItem, currentTargetSlotIndex);
                    ClosePopup();
                });
            }
        }
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}