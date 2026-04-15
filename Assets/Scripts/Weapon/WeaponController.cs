using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Collider weaponCollider; 
    public float currentDamage = 10f;

    public string targetTag = "Enemy";

    // 이번 공격에 맞았던 적 저장
    //private List<Collider> alreadyHitEnemies = new List<Collider>();

    private void Start()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void EnableHitbox()
    {
        weaponCollider.enabled = true;
        //alreadyHitEnemies.Clear(); // 매 공격 시 초기화
    }

    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag(targetTag) /*&& !alreadyHitEnemies.Contains(other)*/)
        {
            //맞았던 적 제외
            //alreadyHitEnemies.Add(other);

            
            if(targetTag == "Enemy")
            {
                EnemyStats hitEnemy = other.GetComponent<EnemyStats>();
                if (hitEnemy != null)
                {
                    hitEnemy.TakeDamage(currentDamage);
                }
            }
            
            
        }
    }
}
