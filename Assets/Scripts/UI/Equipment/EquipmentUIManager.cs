using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class EquipmentUIManager : MonoBehaviour
{
    public static EquipmentUIManager Instance { get; private set; }

    [Header("Main Panels")]
    public GameObject equipmentWindow;      // 장비창 전체 패널 (Tab 메뉴에서 제어)

    [Header("Left Area: Equipment Slots")]
    // 각 타입별 슬롯 배열 (인스펙터에서 드래그 앤 드롭)
    public EquipSlotUI[] weaponSlots;

    public EquipSlotUI headSlot;
    public EquipSlotUI chestSlot;
    public EquipSlotUI legsSlot;
    public EquipSlotUI bootsSlot;

    public EquipSlotUI[] artifactSlots;
    public EquipSlotUI[] consumableSlots;

    [Header("Center Area: Item Tooltip")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI centerItemNameText;
    public TextMeshProUGUI centerItemDescriptionText;
    public TextMeshProUGUI centerItemStatsText;
    public TextMeshProUGUI centerItemScalingText;

    public Image centerItemIconImage;

    [Header("Right Area: Player Status")]
    // ★ 수정: 스크린샷에 맞게 변수들을 완전히 새로 교체합니다!
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI healthText;     // Vigor
    public TextMeshProUGUI staminaText;    // Endurance
    public TextMeshProUGUI strText;        // STR
    public TextMeshProUGUI dexText;        // DEX
    public TextMeshProUGUI totalLifeText;  // Total Life (HP)
    public TextMeshProUGUI totalStaminaText; // Total Stamina
    public TextMeshProUGUI StaminaRegenText; // Stamina Regain


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

    // 0. 창 열기/닫기 (MenuUIManager에서 호출)
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

    // 1. 데이터 동기화 (Refresh UI)
    public void RefreshUI()
    {
        // 무기 슬롯 4개 업데이트
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i].UpdateSlotUI(EquipmentManager.Instance.currentWeapons[i]);
        }

        // ★ 추가: 장신구 슬롯 4개 업데이트
        for (int i = 0; i < artifactSlots.Length; i++)
        {
            artifactSlots[i].UpdateSlotUI(EquipmentManager.Instance.currentAccessories[i]);
        }

        for (int i = 0; i < consumableSlots.Length; i++)
        {
            // (안전장치) 배열 길이를 넘어서지 않게 체크
            if (i < EquipmentManager.Instance.currentConsumables.Length)
            {
                consumableSlots[i].UpdateSlotUI(EquipmentManager.Instance.currentConsumables[i]);
            }
        }

        // 방어구 단일 슬롯들 업데이트
        if (headSlot != null) headSlot.UpdateSlotUI(EquipmentManager.Instance.currentHead);
        if (chestSlot != null) chestSlot.UpdateSlotUI(EquipmentManager.Instance.currentChest);
        if (legsSlot != null) legsSlot.UpdateSlotUI(EquipmentManager.Instance.currentLegs);
        if (bootsSlot != null) bootsSlot.UpdateSlotUI(EquipmentManager.Instance.currentBoots);

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

    // 2. 마우스 Hover 시 중앙 패널(Tooltip) 처리
    public void ShowItemTooltip(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        tooltipPanel.SetActive(true);
        centerItemNameText.text = item.itemData.itemName;
        centerItemDescriptionText.text = item.itemData.description;

        if (centerItemIconImage != null)
        {
            if (item.itemData.icon != null)
            {
                centerItemIconImage.sprite = item.itemData.icon;
                centerItemIconImage.enabled = true; // 이미지 컴포넌트 켜기
            }
            else
            {
                centerItemIconImage.enabled = false; // 아이콘이 없으면 끄기
            }
        }

        if (item.itemData is WeaponData weapon)
        {
            PlayerStats stats = FindFirstObjectByType<PlayerStats>();

            // 1. 등급 계산
            string strGrade = GetScalingGrade(weapon.strScaling);
            string dexGrade = GetScalingGrade(weapon.dexScaling);

            if (stats != null)
            {
                // 2. 실제 총 데미지 계산 (내 현재 스탯 반영)
                float totalDamage = item.GetCalculatedDamage(stats.strength, stats.dexterity);
                float bonusDamage = totalDamage - weapon.baseDamage;

                // 3. 툴팁 텍스트 구성 (등급과 총 데미지 표시)
                centerItemStatsText.text =
                    $"Damage: {weapon.baseDamage} + {bonusDamage:F0} (Total: {totalDamage:F0})\n\n" +
                    $"Stamina cost: {weapon.baseStaminaCost}";

                centerItemScalingText.text = $"STR:  {strGrade}                   DEX:  {dexGrade}\n";
            }
        }
        else if (item.itemData is ConsumableData potion)
        {
            centerItemStatsText.text = $"heal: {potion.healAmount}";
            centerItemScalingText.text = $"STR:  -                   DEX  -\n";
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
            // 1. 코어 스탯 텍스트 연동
            if (levelText != null) levelText.text = $"{stats.level}";
            if (moneyText != null) moneyText.text = $"{stats.currentMoney}";
            if (healthText != null) healthText.text = $"{stats.vigor}";
            if (staminaText != null) staminaText.text = $"{stats.endurance}";
            if (strText != null) strText.text = $"{stats.strength}";
            if (dexText != null) dexText.text = $"{stats.dexterity}";

            // 2. 파생 스탯(전체 체력/스태미나) 연동
            if (totalLifeText != null)
                totalLifeText.text = $"{Mathf.RoundToInt(stats.currentHp)} / {Mathf.RoundToInt(stats.maxHp)}";

            if (totalStaminaText != null)
                totalStaminaText.text = $"{stats.maxStamina}";
            if (StaminaRegenText != null)
                StaminaRegenText.text = $"{stats.staminaRegenRate}";
        }
    }

    public void OpenInventoryPopupForSlot(ItemType slotType, ArmorType armorType, int targetSlotIndex)
    {
        if (EquipSelectionPopupUI.Instance != null)
        {
            // 팝업 매니저에게 세부 부위까지 알려줍니다.
            EquipSelectionPopupUI.Instance.OpenPopup(slotType, armorType, targetSlotIndex);
        }
    }

    private void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshUI;
        }
    }

    private string GetScalingGrade(float scalingValue)
    {
        if (scalingValue >= 15f) return "S"; // 황금색
        if (scalingValue >= 12f) return "A";
        if (scalingValue >= 9f) return "B";
        if (scalingValue >= 6f) return "C";
        if (scalingValue >= 3f) return "D";
        if (scalingValue > 0f) return "E";
        return "-"; // 보정 없음
    }
}