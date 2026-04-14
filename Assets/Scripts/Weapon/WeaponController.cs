using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Collider weaponCollider; 
    public float currentDamage = 10f;

    public string targetTag = "Enemy";

    // 이번 한 번의 휘두르기(스윙)에서 이미 데미지를 준 적들을 기억하는 리스트
    //private List<Collider> alreadyHitEnemies = new List<Collider>();

    private void Start()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    // 공격 시작 시 (FSM에서 호출할 함수)
    public void EnableHitbox()
    {
        weaponCollider.enabled = true;
        //alreadyHitEnemies.Clear(); // 새 공격이 시작됐으니 기억을 지워줍니다.
    }

    // 공격 종료 시 (FSM에서 호출할 함수)
    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag(targetTag) /*&& !alreadyHitEnemies.Contains(other)*/)
        {
            // 명단에 추가해서 이번 스윙에서는 다시 안 맞게 함
            //alreadyHitEnemies.Add(other);

            Debug.Log($"<color=red><b>{other.name}에게 {currentDamage}의 데미지를 입혔습니다!</b></color>");
            if(targetTag == "Enemy")
            {
                EnemyController hitEnemy = other.GetComponent<EnemyController>();
                if (hitEnemy != null)
                {
                    hitEnemy.TakeDamage(currentDamage);
                }
            }
            
            
        }
    }
}
