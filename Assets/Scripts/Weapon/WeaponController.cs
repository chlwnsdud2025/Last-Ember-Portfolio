using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Collider weaponCollider; 
    public float currentDamage = 10f;

    public string targetTag = "Enemy";

    // �̹� �� ���� �ֵθ���(����)���� �̹� �������� �� ������ ����ϴ� ����Ʈ
    //private List<Collider> alreadyHitEnemies = new List<Collider>();

    private void Start()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    // ���� ���� �� (FSM���� ȣ���� �Լ�)
    public void EnableHitbox()
    {
        weaponCollider.enabled = true;
        //alreadyHitEnemies.Clear(); // �� ������ ���۵����� ����� �����ݴϴ�.
    }

    // ���� ���� �� (FSM���� ȣ���� �Լ�)
    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag(targetTag) /*&& !alreadyHitEnemies.Contains(other)*/)
        {
            // ��ܿ� �߰��ؼ� �̹� ���������� �ٽ� �� �°� ��
            //alreadyHitEnemies.Add(other);

            Debug.Log($"<color=red><b>{other.name}���� {currentDamage}�� �������� �������ϴ�!</b></color>");
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
