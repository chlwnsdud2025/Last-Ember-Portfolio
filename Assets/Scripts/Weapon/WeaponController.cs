using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Collider weaponCollider;
    public float currentDamage = 10f;

    // 🚨 다단히트 방지용 리스트 (절대 주석 해제 유지!)
    private List<Collider> alreadyHitEnemies = new List<Collider>();

    private void Start()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void EnableHitbox()
    {
        weaponCollider.enabled = true;
        // 새로운 공격 모션이 시작될 때마다 명부를 싹 비워줍니다.
        alreadyHitEnemies.Clear();
    }

    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 이번 칼질에 이미 맞은 적인가? (다단히트 방지)
        if (alreadyHitEnemies.Contains(other)) return;

        if (other.TryGetComponent<EnemyStats>(out EnemyStats hitEnemy))
        {
            // 2. 명부에 기록해서 이번 스윙에는 더 이상 안 맞게 처리
            alreadyHitEnemies.Add(other);

            // 3. 데미지 주기!
            hitEnemy.TakeDamage(currentDamage);

            // 여기에 타격 이펙트, 역경직, 타격음 재생 코드를 넣으면 완벽합니다!
        }
    }
}