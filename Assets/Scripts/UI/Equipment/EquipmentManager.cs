using System;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("Bones / Anchors")]
    public Transform weaponHolder;

    [Header("Current Equipment")]
    // 4칸짜리 무기 배열로 변경합니다!
    public ItemInstance[] currentWeapons = new ItemInstance[4];
    public ItemInstance currentArmor;
    public ItemInstance currentAccessory;

    public event Action OnEquipmentChanged;

    private PlayerController playerController;
    private GameObject spawnedWeaponModel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        playerController = GetComponent<PlayerController>();
    }

    // 장착할 때 인덱스 번호를 받습니다.
    public void Equip(ItemInstance item, int slotIndex = 0)
    {
        if (item == null || item.itemData == null) return;

        switch (item.itemData.itemType)
        {
            case ItemType.Weapon:
                EquipWeapon(item, slotIndex); // 인덱스 토스
                break;
            case ItemType.Armor:
                currentArmor = item;
                break;
        }

        OnEquipmentChanged?.Invoke();
        Debug.Log($"{item.itemData.itemName}을(를) {slotIndex}번 슬롯에 장착했습니다.");
    }

    // 인덱스를 받아서 배열에 저장합니다.
    private void EquipWeapon(ItemInstance weaponInstance, int slotIndex)
    {
        WeaponData data = weaponInstance.itemData as WeaponData;
        if (data == null) return;

        if (spawnedWeaponModel != null)
        {
            Destroy(spawnedWeaponModel);
        }

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

        // 수정: 단일 변수가 아니라 배열의 [해당 칸]에 저장합니다.
        currentWeapons[slotIndex] = weaponInstance;

        playerController.EquipWeapon(data.attackData);

        float finalDamage = weaponInstance.GetCalculatedDamage(10, 10);
        if (playerController.currentWeaponScript != null)
        {
            playerController.currentWeaponScript.currentDamage = finalDamage;
        }
    }

    public void Unequip(ItemType type, int slotIndex = 0)
    {
        if (type == ItemType.Weapon)
        {
            // 손에 쥐고 있던 3D 모델링 파괴
            if (spawnedWeaponModel != null) Destroy(spawnedWeaponModel);

            // 배열에서 해당 칸을 비워줍니다!
            currentWeapons[slotIndex] = null;

            // (선택) 맨손 애니메이션으로 복구하는 로직
            // playerController.EquipWeapon(기본_맨손_데이터);
        }
        else if (type == ItemType.Armor)
        {
            currentArmor = null;
        }
        else if (type == ItemType.Artifact) // 또는 Accessory
        {
            currentAccessory = null;
        }

        // UI 매니저에게 장비가 빠졌다고 알려서 화면 갱신
        OnEquipmentChanged?.Invoke();
    }
}