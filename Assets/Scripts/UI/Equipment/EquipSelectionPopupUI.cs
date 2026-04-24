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
    public void OpenPopup(ItemType mainType, ArmorType subType, int slotIndex)
    {
        currentTargetSlotIndex = slotIndex;
        UIManager.Instance.OpenPanel(popupPanel);
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        // 1. 일단 대분류(Armor, Weapon 등)로 1차 필터링
        List<ItemInstance> filteredItems = InventoryManager.Instance.GetItemsByType(mainType);

        // 2. ★ 고도화: 만약 방어구라면 소분류(머리, 가슴 등)로 2차 필터링
        if (mainType == ItemType.Armor)
        {
            filteredItems = filteredItems.FindAll(item =>
                (item.itemData as ArmorData)?.armorCategory == subType
            );
        }

        // 3. 슬롯 생성 (기존과 동일)
        foreach (ItemInstance item in filteredItems)
        {
            GameObject newSlotObj = Instantiate(inventorySlotPrefab, scrollContent);
            InventorySlotUI slotUI = newSlotObj.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.InitSlot(item, (selectedItem) =>
                {
                    EquipmentManager.Instance.Equip(selectedItem, currentTargetSlotIndex);
                    ClosePopup();
                });
            }
        }
    }

    public void ClosePopup()
    {
        UIManager.Instance.ClosePanel(popupPanel);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}