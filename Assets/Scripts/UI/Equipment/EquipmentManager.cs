using System;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("Bones / Anchors")]
    public Transform weaponHolder; // 무기가 실제로 생성되어 붙을 손 위치 (Bone)

    [Header("Current Equipment")]
    public ItemInstance currentWeapon;
    // 향후 확장을 위해 미리 정의
    public ItemInstance currentArmor;
    public ItemInstance currentAccessory;

    // 장비가 바뀔 때마다 UI나 다른 시스템에 알림
    public event Action OnEquipmentChanged;

    private PlayerController playerController;
    private GameObject spawnedWeaponModel; // 현재 손에 들고 있는 무기 프리팹 인스턴스

    private void Awake()
    {
        if (Instance == null) Instance = this;
        playerController = GetComponent<PlayerController>();
    }

    /// <summary>
    /// 아이템을 특정 슬롯에 장착합니다.
    /// </summary>
    public void Equip(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        switch (item.itemData.itemType)
        {
            case ItemType.Weapon:
                EquipWeapon(item);
                break;
            case ItemType.Armor:
                currentArmor = item;
                // 방어구 스탯 적용 로직 추가 가능
                break;
                // 추가 케이스...
        }

        OnEquipmentChanged?.Invoke();
        Debug.Log($"{item.itemData.itemName}을(를) 장착했습니다.");
    }

    private void EquipWeapon(ItemInstance weaponInstance)
    {
        WeaponData data = weaponInstance.itemData as WeaponData;
        if (data == null) return;

        // 1. 기존 무기 모델 제거
        if (spawnedWeaponModel != null)
        {
            Destroy(spawnedWeaponModel);
        }

        // 2. 새 무기 모델 생성 및 손에 부착
        if (data.weaponPrefab != null && weaponHolder != null)
        {
            spawnedWeaponModel = Instantiate(data.weaponPrefab, weaponHolder);
            spawnedWeaponModel.transform.localPosition = Vector3.zero;
            spawnedWeaponModel.transform.localRotation = Quaternion.identity;

            // WeaponController 연결 (공격 판정을 위해)
            WeaponController weaponScript = spawnedWeaponModel.GetComponent<WeaponController>();
            if (weaponScript != null)
            {
                playerController.currentWeaponScript = weaponScript;
            }
        }

        // 3. 데이터 업데이트
        currentWeapon = weaponInstance;

        // 4. PlayerController의 애니메이션/공격 데이터 갱신
        playerController.EquipWeapon(data.attackData);

        // 5. 무기 공격력 계산 (스탯 반영)
        // 현재 무기의 실제 공격력을 PlayerController의 데미지로 전달
        float finalDamage = weaponInstance.GetCalculatedDamage(10, 10); // 임시 스탯 값
        if (playerController.currentWeaponScript != null)
        {
            playerController.currentWeaponScript.currentDamage = finalDamage;
        }
    }

    public void Unequip(ItemType type)
    {
        if (type == ItemType.Weapon)
        {
            if (spawnedWeaponModel != null) Destroy(spawnedWeaponModel);
            currentWeapon = null;
            // 기본 맨손 상태로 복구 로직 추가 가능
        }
        OnEquipmentChanged?.Invoke();
    }
}