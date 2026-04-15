using UnityEngine;

public class SkeletonBossController : BossController
{
    [Header("Knight Specific")]
    public float smashDamage = 50f;
    //public ParticleSystem earthquakeEffect; // 내려찍을 때 터질 이펙트!
    //public Weapon bossWeapon; // 보스가 든 거대한 무기

    protected override void Awake()
    {
        base.Awake();
        // (상태 초기화 로직...)
    }
}