using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 마우스 오버 이벤트를 위해 추가!
using TMPro;

// IPointerEnterHandler, IPointerExitHandler 인터페이스를 상속받습니다.
public class EquipSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemType targetSlotType;
    public int slotIndex; // 무기 1, 무기 2 구분을 위해 인덱스 추가 (0~3)

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

        if (item != null)
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
        // 팝업 인벤토리를 열 때, 자기가 몇 번째 슬롯인지 매니저에게 알려줍니다.
        EquipmentUIManager.Instance.OpenInventoryPopupForSlot(targetSlotType, slotIndex);
    }

    // 마우스가 슬롯 안으로 들어왔을 때 (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentEquippedItem != null)
        {
            // 중앙 패널에 이 아이템의 정보를 띄우라고 지시!
            EquipmentUIManager.Instance.ShowItemTooltip(currentEquippedItem);
        }
    }

    // 마우스가 슬롯 밖으로 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        // 중앙 패널 정보 지우기
        EquipmentUIManager.Instance.HideItemTooltip();
    }
}