using System.Collections.Generic;
using UnityEngine;

// 이 스크립트는 보스의 무기나 주먹(콜라이더가 있는 오브젝트)에 직접 붙입니다.
public class BossMeleeHitbox : MonoBehaviour
{
    private BossHitboxManager hitboxManager;
    private List<Collider> alreadyHitList = new List<Collider>();

    private void Start()
    {
        hitboxManager = GetComponentInParent<BossHitboxManager>();
    }

    //  매니저가 공격을 시작할 때마다 직접 호출해줄 함수!
    public void ClearHitList()
    {
        alreadyHitList.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !alreadyHitList.Contains(other))
        {
            alreadyHitList.Add(other); // 타격 명부에 기록

            float damageToDeal = hitboxManager != null ? hitboxManager.currentDamage : 10f;
            Debug.Log($"[보스 근접 타격] 플레이어 적중! 데미지: {damageToDeal}");

            PlayerController hitPlayer = other.GetComponent<PlayerController>();
            if (hitPlayer != null)
            {
                hitPlayer.TakeDamage(damageToDeal);
            }
        }
    }
}