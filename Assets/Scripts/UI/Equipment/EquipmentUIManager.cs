using UnityEngine;
using TMPro;

public class EquipmentUIManager : MonoBehaviour
{
    public static EquipmentUIManager Instance { get; private set; }

    [Header("Main Panels")]
    public GameObject equipmentWindow;      // 장비창 전체 패널 (Tab 메뉴에서 제어)

    [Header("Left Area: Equipment Slots")]
    // 각 타입별 슬롯 배열 (인스펙터에서 드래그 앤 드롭)
    public EquipSlotUI[] weaponSlots;
    public EquipSlotUI[] armorSlots;
    public EquipSlotUI[] artifactSlots;
    public EquipSlotUI[] consumableSlots;

    [Header("Center Area: Item Tooltip")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI centerItemNameText;
    public TextMeshProUGUI centerItemDescriptionText;
    public TextMeshProUGUI centerItemStatsText;

    [Header("Right Area: Player Status")]
    public TextMeshProUGUI playerHpText;
    public TextMeshProUGUI playerStaminaText;
    public TextMeshProUGUI playerTotalDamageText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // 시작 시 초기화
        if (equipmentWindow != null) equipmentWindow.SetActive(false);

        HideItemTooltip();
        UpdatePlayerStatusUI();

        // EquipmentManager의 데이터가 변경될 때마다 UI를 새로고침하도록 구독
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged += RefreshUI;
        }
    }

    // ==========================================
    // 0. 창 열기/닫기 (MenuUIManager에서 호출)
    // ==========================================
    public void OpenWindow()
    {
        if (equipmentWindow != null)
        {
            equipmentWindow.SetActive(true);
            RefreshUI(); // 열 때 최신 데이터 반영
        }
    }

    public void CloseWindow()
    {
        if (equipmentWindow != null)
        {
            equipmentWindow.SetActive(false);
            HideItemTooltip();
        }
    }

    // ==========================================
    // 1. 데이터 동기화 (Refresh UI)
    // ==========================================
    public void RefreshUI()
    {
        // ★ 수정: 무기 슬롯 4칸을 돌면서, Manager의 배열 데이터를 1:1로 매칭해줍니다!
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i].UpdateSlotUI(EquipmentManager.Instance.currentWeapons[i]);
        }

        // 방어구, 장신구는 아직 1칸이므로 0번째 슬롯에만 세팅
        if (armorSlots.Length > 0) armorSlots[0].UpdateSlotUI(EquipmentManager.Instance.currentArmor);
        if (artifactSlots.Length > 0) artifactSlots[0].UpdateSlotUI(EquipmentManager.Instance.currentAccessory);

        UpdatePlayerStatusUI();
    }

    private void UpdateSlotArray(EquipSlotUI[] slots, ItemInstance item)
    {
        if (slots == null || slots.Length == 0) return;

        // 현재 데이터 구조상 첫 번째 슬롯(0번)에만 장착 정보를 표시하고 나머지는 비웁니다.
        slots[0].UpdateSlotUI(item);

        for (int i = 1; i < slots.Length; i++)
        {
            slots[i].UpdateSlotUI(null);
        }
    }

    // ==========================================
    // 2. 마우스 Hover 시 중앙 패널(Tooltip) 처리
    // ==========================================
    public void ShowItemTooltip(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        tooltipPanel.SetActive(true);
        centerItemNameText.text = item.itemData.itemName;
        centerItemDescriptionText.text = item.itemData.description;

        if (item.itemData is WeaponData weapon)
        {
            centerItemStatsText.text = $"공격력: {weapon.baseDamage} \n스태미나 소모: {weapon.baseStaminaCost}";
        }
        else if (item.itemData is ConsumableData potion)
        {
            centerItemStatsText.text = $"회복량: {potion.healAmount}";
        }
    }

    public void HideItemTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    // ==========================================
    // 3. 우측 플레이어 스탯 패널 업데이트
    // ==========================================
    public void UpdatePlayerStatusUI()
    {
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            if (playerHpText != null) playerHpText.text = $"HP: {stats.healAmount} (Heal Pwr)";
            if (playerStaminaText != null) playerStaminaText.text = $"STAMINA: {stats.CurrentStamina:F0}";

            if (playerTotalDamageText != null)
            {
                // ★ 수정: 현재는 '1번 무기(인덱스 0)'의 데미지만 표시하도록 임시 설정
                if (EquipmentManager.Instance.currentWeapons[0] != null)
                {
                    float damage = EquipmentManager.Instance.currentWeapons[0].GetCalculatedDamage(10, 10);
                    playerTotalDamageText.text = $"DAMAGE: {damage}";
                }
                else
                {
                    playerTotalDamageText.text = "DAMAGE: 0";
                }
            }
        }
    }

    public void OpenInventoryPopupForSlot(ItemType slotType, int targetSlotIndex)
    {
        if (EquipSelectionPopupUI.Instance != null)
        {
            EquipSelectionPopupUI.Instance.OpenPopup(slotType, targetSlotIndex);
        }
    }

    private void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshUI;
        }
    }
}