using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquipSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Settings")]
    public ItemType targetSlotType;       // 대분류 (Weapon, Armor 등)
    public ArmorType targetArmorType;     // 소분류 (Head, Chest 등 - 방어구가 아니면 무시됨)
    public int slotIndex;                 // 슬롯 번호 (무기 0~3, 장신구 0~3)

    [Header("UI Components")]
    public Image iconImage;
    public Button slotButton;

    private ItemInstance currentEquippedItem;

    private void Awake()
    {
        slotButton.onClick.AddListener(OnSlotClicked);
    }

    public void UpdateSlotUI(ItemInstance item)
    {
        currentEquippedItem = item;

        // 안전장치: Image 컴포넌트 연결 누락 방지
        if (iconImage == null)
        {
            Debug.LogError($"[UI 버그] {gameObject.name}의 EquipSlotUI에 'Icon Image'가 연결되지 않았습니다!", gameObject);
            return;
        }

        if (item != null && item.itemData != null)
        {
            iconImage.sprite = item.itemData.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    private void OnSlotClicked()
    {
        // ★ 에러 해결: 대분류, 소분류, 슬롯번호 3가지를 정확하게 매니저에게 전달합니다!
        EquipmentUIManager.Instance.OpenInventoryPopupForSlot(targetSlotType, targetArmorType, slotIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentEquippedItem != null)
        {
            EquipmentUIManager.Instance.ShowItemTooltip(currentEquippedItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }
}