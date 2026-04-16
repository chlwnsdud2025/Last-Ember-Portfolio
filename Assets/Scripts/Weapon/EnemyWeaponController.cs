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
        //  [핵심 2] 이미 히트박스가 켜져 있다면 함수를 종료합니다! 
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
        //  [핵심 3] 리스트에 없는 대상(이번 스윙에 아직 안 맞은 대상)일 때만 데미지를 줍니다.
        if (other.CompareTag(targetTag) && !alreadyHitList.Contains(other))
        {
            alreadyHitList.Add(other); // "이 녀석은 이미 때렸다!" 하고 명부에 적어둡니다.

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