using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class QuickSlotUIManager : MonoBehaviour
{
    public static QuickSlotUIManager Instance { get; private set; }

    [Header("UI Icons")]
    public Image currentWeaponIcon;     // 좌측 하단 Weapon 칸의 아이콘
    public Image currentConsumableIcon; // 좌측 하단 Item 칸의 아이콘


    [Header("UI Texts")]
    public TextMeshProUGUI consumableCountText;


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (EquipmentManager.Instance != null)
        {
            // 이벤트 구독
            EquipmentManager.Instance.OnEquipmentChanged += RefreshQuickSlots;

            // 시작하자마자 현재 장착 상태를 화면에 표시!
            RefreshQuickSlots();
        }
    }

    public void RefreshQuickSlots()
    {
        // EquipmentManager에서 '현재 선택된' 슬롯 번호를 가져옵니다.
        int wIdx = EquipmentManager.Instance.activeWeaponIndex;
        int cIdx = EquipmentManager.Instance.activeConsumableIndex;

        // 해당 번호에 장착된 아이템 데이터를 가져옵니다.
        ItemInstance activeWeapon = EquipmentManager.Instance.currentWeapons[wIdx];
        ItemInstance activeItem = EquipmentManager.Instance.currentConsumables[cIdx];

        // UI 업데이트
        UpdateIcon(currentWeaponIcon, activeWeapon, null);
        UpdateIcon(currentConsumableIcon, activeItem, consumableCountText);
    }

    private void UpdateIcon(Image iconImage, ItemInstance item, TextMeshProUGUI countText)
    {
        if (iconImage == null) return;

        // 아이템이 있고, 개수가 0보다 크면 표시
        if (item != null && item.itemData != null && item.amount > 0)
        {
            iconImage.sprite = item.itemData.icon;
            iconImage.enabled = true;

            if (countText != null)
            {
                countText.text = item.amount.ToString();
                countText.gameObject.SetActive(true); // 숫자 켜기
            }
        }
        else
        {
            iconImage.enabled = false;
            if (countText != null) countText.gameObject.SetActive(false); // 빈칸이거나 0개면 숫자 끄기
        }
    }

    private void OnDestroy()
    {
        // 1. 이벤트 구독 안전하게 해제 (EquipmentManager가 살아있을 때만)
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshQuickSlots;
        }

        // 2. ★ 추가: 싱글톤 인스턴스 초기화 (유령 메모리 참조 방지)
        if (Instance == this)
        {
            Instance = null;
        }
    }
}