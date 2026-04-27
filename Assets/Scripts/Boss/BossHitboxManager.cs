using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitboxMapping
{
    public HitboxType type;
    public Collider hitboxCollider;
}

public class BossHitboxManager : MonoBehaviour
{
    [Header("보스의 타격 부위들을 연결해주세요")]
    public List<HitboxMapping> hitboxes = new List<HitboxMapping>();

    [HideInInspector] public float currentDamage = 0f;

    private void Start()
    {
        DisableAllHitboxes();
    }

    public void EnableHitbox(HitboxType type)
    {
        foreach (var mapping in hitboxes)
        {
            if (mapping.type == type && mapping.hitboxCollider != null)
            {
                // 1. 콜라이더 판정 켜기
                mapping.hitboxCollider.enabled = true;

                // 2. 타격 기록 싹 지우기 (여기 추가됨!)
                BossMeleeHitbox hitboxScript = mapping.hitboxCollider.GetComponent<BossMeleeHitbox>();
                if (hitboxScript != null)
                {
                    hitboxScript.ClearHitList();
                }
            }
        }
    }

    public void DisableAllHitboxes()
    {
        foreach (var mapping in hitboxes)
        {
            if (mapping.hitboxCollider != null)
            {
                mapping.hitboxCollider.enabled = false;
            }
        }
    }
}