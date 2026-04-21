using UnityEngine;

[System.Serializable] 
public class ItemInstance
{
    // --- 직렬화 되어 세이브 파일에 기록될 가변 데이터 ---
    public string itemID;      // 어떤 아이템인지 ID만 기억함 (데이터베이스 로드용)
    public int amount;         // 현재 소지 개수
    public int upgradeLevel;   // 무기 강화 수치 (ex: +3)

    // --- 런타임 캐싱 데이터 (세이브 파일에 저장 안 함) ---
    [System.NonSerialized]
    public ItemDataBase itemData; // 런타임에 ID를 기반으로 찾아올 실제 SO 원본 데이터

    // 생성자: 아이템을 처음 획득할 때 호출
    public ItemInstance(ItemDataBase data, int amount = 1)
    {
        this.itemData = data;
        this.itemID = data.itemID;
        this.amount = amount;

        this.upgradeLevel = 0;
    }

    // 소울라이크 식 실 데미지 계산 로직 예시
    public float GetCalculatedDamage(float playerStr, float playerDex)
    {
        if (itemData is WeaponData weapon)
        {
            float baseDmg = weapon.baseDamage + (upgradeLevel * 5f); // 강화당 5 증가
            float scalingDmg = (playerStr * weapon.strScaling) + (playerDex * weapon.dexScaling);
            return baseDmg + scalingDmg;
        }
        return 0;
    }
}