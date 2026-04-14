using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{

    public Collider weaponCollider;
    public float currentDamage = 10f;

    public string targetTag = "Player";

    private Collider alreadyHit;
    private void Start()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    // 공격 시작 시 (FSM에서 호출할 함수)
    public void EnableHitbox()
    {
        weaponCollider.enabled = true;
        alreadyHit = null;
    }

    // 공격 종료 시 (FSM에서 호출할 함수)
    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(targetTag) && alreadyHit == null)
        {
            // 명단에 추가해서 이번 스윙에서는 다시 안 맞게 함
            alreadyHit = other;

            if (targetTag == "Player")
            {
                PlayerController hitPlayer = other.GetComponent<PlayerController>();

                hitPlayer.TakeDamage(currentDamage);

            }

        }
    }
}
