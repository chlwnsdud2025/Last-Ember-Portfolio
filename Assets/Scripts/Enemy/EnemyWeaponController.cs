using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{

    public Collider weaponCollider;
    [SerializeField] private float damage = 10f; // 적의 데미지

    public string targetTag = "Player";
    private Collider alreadyHit;

    private void Start()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void EnableHitbox()
    {
        weaponCollider.enabled = true;
        alreadyHit = null;
    }

    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) && alreadyHit == null)
        {
            alreadyHit = other;

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
