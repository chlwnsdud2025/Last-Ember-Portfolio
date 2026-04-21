using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon Data")]
public class WeaponData : ItemDataBase
{
    [Header("Weapon Stats")]
    public float baseDamage;
    public float baseStaminaCost;

    [Header("Combat System")]
    public Attack_SO_Data attackData; // 콤보 애니메이션 연동
    public GameObject weaponPrefab;   // 손에 쥐어줄 3D 모델

    [Header("Soul-like Scaling")] // 스텟 보정치 아직 구현 x
    public float strScaling; 
    public float dexScaling; 
}