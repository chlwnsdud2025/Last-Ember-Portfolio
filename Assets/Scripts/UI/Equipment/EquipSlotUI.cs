using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// ★ 수정: IPointerClickHandler 인터페이스 추가
public class EquipSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
        // ★ 수정: 기존의 버튼 onClick 리스너는 삭제합니다. (OnPointerClick에서 모두 처리할 예정)
        // slotButton.onClick.AddListener(OnSlotClicked); 
    }

    public void UpdateSlotUI(ItemInstance item)
    {
        currentEquippedItem = item;

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

    // ★ 추가: 마우스 클릭 이벤트를 감지하여 좌/우클릭을 구분합니다.
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. 좌클릭: 장비 교체 팝업 열기
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            EquipmentUIManager.Instance.OpenInventoryPopupForSlot(targetSlotType, targetArmorType, slotIndex);
        }
        // 2. 우클릭: 장비 해제
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentEquippedItem != null)
            {
                // EquipmentManager의 Unequip 함수 호출
                EquipmentManager.Instance.Unequip(targetSlotType, slotIndex, targetArmorType);

                // 장비가 해제되었으니 툴팁도 즉시 숨겨줍니다.
                EquipmentUIManager.Instance.HideItemTooltip();
            }
        }
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
        // (필요 시) 마우스가 슬롯 밖으로 나갈 때 툴팁을 끄는 로직을 여기에 넣어도 좋습니다.
        // EquipmentUIManager.Instance.HideItemTooltip();
    }
}