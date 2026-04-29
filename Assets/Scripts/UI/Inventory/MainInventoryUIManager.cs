using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainInventoryUIManager : MonoBehaviour
{
    public static MainInventoryUIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject inventoryWindow; // 인벤토리 창 전체 패널
    public Transform scrollContent;    // 아이템이 뜰 Grid Layout Group 영역
    public GameObject inventorySlotPrefab;

    [Header("Item Details")]
    public Image detailIconImage;
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailDescText;
    public TextMeshProUGUI detailStatsText;
    // ★ 추가: 장비창처럼 S, A, B 등급을 표시할 텍스트
    public TextMeshProUGUI detailScalingText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (inventoryWindow != null) inventoryWindow.SetActive(false);
        ClearItemDetails();
    }

    // 1. 창 열기 / 닫기
    public void OpenWindow()
    {
        UIManager.Instance.OpenPanel(inventoryWindow);
        ShowItemsByCategory(null);
    }

    public void CloseWindow()
    {
        UIManager.Instance.ClosePanel(inventoryWindow);
    }

    // 2. 탭(Tab) 버튼 기능
    public void OnClickAllTab() { ShowItemsByCategory(null); }
    public void OnClickWeaponTab() { ShowItemsByCategory(ItemType.Weapon); }
    public void OnClickArmorTab() { ShowItemsByCategory(ItemType.Armor); }
    public void OnClickArtifactTab() { ShowItemsByCategory(ItemType.Artifact); }
    public void OnClickConsumableTab() { ShowItemsByCategory(ItemType.Consumable); }

    private void ShowItemsByCategory(ItemType? categoryFilter)
    {
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        List<ItemInstance> itemsToShow = InventoryManager.Instance.allItems;

        if (categoryFilter.HasValue)
        {
            itemsToShow = InventoryManager.Instance.GetItemsByType(categoryFilter.Value);
        }

        foreach (ItemInstance item in itemsToShow)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, scrollContent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

            if (slotUI != null)
            {
                slotUI.InitSlot(item, ShowItemDetails);
            }
        }
    }

    // ==========================================
    // ★ 수정: 장비창(EquipmentUIManager)과 완벽하게 동일한 로직 적용!
    // ==========================================
    private void ShowItemDetails(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        // 아이콘 표시
        if (detailIconImage != null)
        {
            if (item.itemData.icon != null)
            {
                detailIconImage.sprite = item.itemData.icon;
                detailIconImage.enabled = true;
            }
            else
            {
                detailIconImage.enabled = false;
            }
        }

        detailNameText.text = item.itemData.itemName;
        detailDescText.text = item.itemData.description;

        // 무기일 경우 데미지 계산 및 스케일링 등급 표시
        if (item.itemData is WeaponData weapon)
        {
            PlayerStats stats = FindFirstObjectByType<PlayerStats>();

            string strGrade = GetScalingGrade(weapon.strScaling);
            string dexGrade = GetScalingGrade(weapon.dexScaling);

            if (stats != null)
            {
                float totalDamage = item.GetCalculatedDamage(stats.strength, stats.dexterity);
                float bonusDamage = totalDamage - weapon.baseDamage;

                // 인벤토리이므로 보유 개수(Count)도 추가로 보여줍니다.
                detailStatsText.text =
                    $"Damage: {weapon.baseDamage} + {bonusDamage:F0} (Total: {totalDamage:F0})\n\n" +
                    $"Stamina cost: {weapon.baseStaminaCost}\n\n" +
                    $"Count: {item.amount}";

                if (detailScalingText != null)
                    detailScalingText.text = $"STR:  {strGrade}                   DEX:  {dexGrade}\n";
            }
        }
        // 소모품일 경우
        else if (item.itemData is ConsumableData potion)
        {
            detailStatsText.text = $"Heal: {potion.healAmount}\n\nCount: {item.amount}";
            if (detailScalingText != null)
                detailScalingText.text = $"STR:  -                   DEX:  -\n";
        }
        // 그 외 아이템 (방어구, 장신구 등)
        else
        {
            detailStatsText.text = $"Count: {item.amount}";
            if (detailScalingText != null)
                detailScalingText.text = "";
        }
    }

    //등급 계산 함수
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

    private void ClearItemDetails()
    {
        detailNameText.text = "";
        detailDescText.text = "Select Item.";
        detailStatsText.text = "";
        if (detailScalingText != null) detailScalingText.text = "";
        if (detailIconImage != null) detailIconImage.enabled = false;
    }
}