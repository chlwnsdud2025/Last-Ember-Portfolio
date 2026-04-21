using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action을 사용하기 위해 추가

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Button slotButton;

    private ItemInstance myItem;
    private Action<ItemInstance> onClickAction; // 클릭 시 실행할 함수를 저장할 변수

    // InitSlot을 호출할 때, "이 버튼 누르면 무슨 행동을 할지"를 매개변수로 같이 넘겨받습니다.
    public void InitSlot(ItemInstance item, Action<ItemInstance> onClickCallback)
    {
        myItem = item;
        onClickAction = onClickCallback;

        iconImage.sprite = item.itemData.icon;
        nameText.text = item.itemData.itemName;

        // 기존에 연결된 이벤트가 있다면 싹 지우고 새로 연결 (버그 방지)
        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        // 내장된 동작이 아니라, 외부에서 주입받은 동작을 실행!
        onClickAction?.Invoke(myItem);
    }
}