using UnityEngine;
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Data")]
public class weapon_1_ob : ScriptableObject
{
    public string weaponName;

    [Header("Combo Clips")]
    // 인스펙터에서 애니메이션 파일을 직접 슬롯에 넣습니다.
    public AnimationClip[] comboClips;

    [Header("Stats")]
    public float baseDamage = 10f;
}
