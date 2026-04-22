using System;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("Bones / Anchors")]
    public Transform weaponHolder;

    [Header("Current Equipment")]
    public ItemInstance[] currentWeapons = new ItemInstance[4];     // 무기 4칸
    public ItemInstance currentHead;
    public ItemInstance currentChest;
    public ItemInstance currentLegs;
    public ItemInstance currentBoots;
    public ItemInstance[] currentAccessories = new ItemInstance[4]; // 장신구 4칸
    public ItemInstance[] currentConsumables = new ItemInstance[8]; // 소비템 8칸

    [Header("Current Active Slots (0~3 / 0~7)")]
    public int activeWeaponIndex = 0;
    public int activeConsumableIndex = 0;

    public ItemInstance currentAccessory;
    public event Action OnEquipmentChanged;

    private PlayerController playerController;
    private GameObject spawnedWeaponModel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) CycleWeapon(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) CycleWeapon(-1);

        if (Input.GetKeyDown(KeyCode.DownArrow)) CycleConsumable(1);
        if (Input.GetKeyDown(KeyCode.UpArrow)) CycleConsumable(-1);
    }

    // 
    // 무기 스위칭
    private void CycleWeapon(int direction)
    {
        int originalIndex = activeWeaponIndex;
        int nextIndex = activeWeaponIndex;
        int maxSlots = currentWeapons.Length;
        bool found = false;

        for (int i = 0; i < maxSlots; i++)
        {
            nextIndex += direction;
            if (nextIndex >= maxSlots) nextIndex = 0;
            else if (nextIndex < 0) nextIndex = maxSlots - 1;

            // ★ 핵심 수정: 껍데기가 아닌 '진짜 데이터(itemData)'가 있는지까지 검사!
            if (currentWeapons[nextIndex] != null && currentWeapons[nextIndex].itemData != null)
            {
                found = true;
                break; // 진짜 아이템을 찾았으니 여기서 스킵 정지!
            }
        }

        // 4칸을 다 뒤졌는데도 알맹이가 하나도 없다면 헛돌지 않게 무시
        if (!found) return;

        activeWeaponIndex = nextIndex;
        ItemInstance newWeapon = currentWeapons[activeWeaponIndex];

        if (spawnedWeaponModel != null) Destroy(spawnedWeaponModel);
        if (newWeapon != null)
        {
            EquipWeapon(newWeapon, activeWeaponIndex);
        }

        OnEquipmentChanged?.Invoke();
    }

    // 소비템 스위칭
    private void CycleConsumable(int direction)
    {
        int originalIndex = activeConsumableIndex;
        int nextIndex = activeConsumableIndex;
        int maxSlots = currentConsumables.Length;
        bool found = false;

        for (int i = 0; i < maxSlots; i++)
        {
            nextIndex += direction;
            if (nextIndex >= maxSlots) nextIndex = 0;
            else if (nextIndex < 0) nextIndex = maxSlots - 1;

            // ★ 핵심 수정: 껍데기가 아닌 '진짜 데이터(itemData)'가 있는지까지 검사!
            if (currentConsumables[nextIndex] != null && currentConsumables[nextIndex].itemData != null)
            {
                found = true;
                break;
            }
        }

        if (!found) return;

        activeConsumableIndex = nextIndex;
        OnEquipmentChanged?.Invoke();
    }

    // ==========================================
    // ★ 수정됨: 장착 로직 (장착 시 해당 번호로 퀵슬롯 즉시 이동)
    // ==========================================
    public void Equip(ItemInstance item, int slotIndex = 0)
    {
        if (item == null || item.itemData == null) return;

        switch (item.itemData.itemType)
        {
            case ItemType.Weapon:
                EquipWeapon(item, slotIndex);
                activeWeaponIndex = slotIndex; // 추가: 장착한 무기로 퀵슬롯 화면 이동
                break;
            case ItemType.Armor:
                EquipArmor(item);
                break;
            case ItemType.Artifact:
                currentAccessories[slotIndex] = item;
                break;
            case ItemType.Consumable:
                currentConsumables[slotIndex] = item;
                activeConsumableIndex = slotIndex; // 추가: 장착한 소비템으로 퀵슬롯 화면 이동
                break;
        }

        OnEquipmentChanged?.Invoke();
    }

    private void EquipWeapon(ItemInstance weaponInstance, int slotIndex)
    {
        WeaponData data = weaponInstance.itemData as WeaponData;
        if (data == null) return;

        if (spawnedWeaponModel != null) Destroy(spawnedWeaponModel);

        if (data.weaponPrefab != null && weaponHolder != null)
        {
            spawnedWeaponModel = Instantiate(data.weaponPrefab, weaponHolder);
            spawnedWeaponModel.transform.localPosition = Vector3.zero;
            spawnedWeaponModel.transform.localRotation = Quaternion.identity;

            WeaponController weaponScript = spawnedWeaponModel.GetComponent<WeaponController>();
            if (weaponScript != null)
            {
                playerController.currentWeaponScript = weaponScript;
            }
        }

        currentWeapons[slotIndex] = weaponInstance;
        playerController.EquipWeapon(data.attackData);

        float finalDamage = weaponInstance.GetCalculatedDamage(10, 10);
        if (playerController.currentWeaponScript != null)
        {
            playerController.currentWeaponScript.currentDamage = finalDamage;
        }
    }

    private void EquipArmor(ItemInstance armorInstance)
    {
        ArmorData data = armorInstance.itemData as ArmorData;
        if (data == null) return;

        switch (data.armorCategory)
        {
            case ArmorType.Head: currentHead = armorInstance; break;
            case ArmorType.Chest: currentChest = armorInstance; break;
            case ArmorType.Legs: currentLegs = armorInstance; break;
            case ArmorType.Boots: currentBoots = armorInstance; break;
        }
    }

    public void Unequip(ItemType type, int slotIndex = 0, ArmorType? armorType = null)
    {
        if (type == ItemType.Weapon)
        {
            if (spawnedWeaponModel != null) Destroy(spawnedWeaponModel);
            currentWeapons[slotIndex] = null;
        }
        else if (type == ItemType.Armor && armorType.HasValue)
        {
            switch (armorType.Value)
            {
                case ArmorType.Head: currentHead = null; break;
                case ArmorType.Chest: currentChest = null; break;
                case ArmorType.Legs: currentLegs = null; break;
                case ArmorType.Boots: currentBoots = null; break;
            }
        }
        else if (type == ItemType.Artifact)
        {
            currentAccessories[slotIndex] = null;
        }
        else if (type == ItemType.Consumable)
        {
            currentConsumables[slotIndex] = null;
        }

        OnEquipmentChanged?.Invoke();
    }
}