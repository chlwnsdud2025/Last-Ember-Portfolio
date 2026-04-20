using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{
    public Collider weaponCollider;
    [SerializeField] private float damage = 10f; // 적의 데미지

    public string targetTag = "Player";

    //  [핵심 1] 단일 변수 대신 List를 사용하여 한 번의 공격(스윙)에 맞은 '모든' 대상을 기억합니다.
    private List<Collider> alreadyHitList = new List<Collider>();

    private void Start()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void EnableHitbox()
    {
        // (State의 Update문에서 매 프레임 호출하더라도 리스트가 계속 지워지는 것을 방지)
        if (weaponCollider.enabled) return;

        weaponCollider.enabled = true;
        alreadyHitList.Clear(); // 켤 때만 타격 기록을 깨끗하게 지웁니다.
    }

    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) && !alreadyHitList.Contains(other))
        {
            alreadyHitList.Add(other); 

            if (targetTag == "Player")
            {
                PlayerController hitPlayer = other.GetComponent<PlayerController>();
                if (hitPlayer != null)
                {
                    hitPlayer.TakeDamage(damage);
                }
            }
        }
    }
}