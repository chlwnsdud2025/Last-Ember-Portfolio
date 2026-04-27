using UnityEngine;

public class SpawnedAttack : MonoBehaviour
{
    public enum SpawnType
    {
        Projectile,   // 앞으로 날아가는 투사체 (파이어볼, 화살 등)
        StationaryAoE // 제자리에 머무는 광역기/장판 (지진, 폭발, 독 구름 등)
    }

    [Header("기본 설정")]
    public SpawnType attackType = SpawnType.Projectile;
    public float lifeTime = 3f;
    public float damage = 20f;

    [Header("이동 및 크기 설정")]
    [Tooltip("Projectile일 경우 날아가는 속도")]
    public float speed = 15f;

    [Tooltip("0보다 크면, 광역기가 서서히 커집니다 (충격파 효과)")]
    public float expandSpeed = 0f;

    [Header("충돌 설정")]
    [Tooltip("체크하면 플레이어와 닿는 즉시 파괴됩니다. (관통하는 검기나 장판은 체크 해제)")]
    public bool destroyOnHit = true;

    private void Start()
    {
        // 수명이 다하면 무조건 파괴 (메모리 누수 방지)
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 1. 투사체 모드일 때만 앞으로 이동
        if (attackType == SpawnType.Projectile)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        // 2. 팽창 속도가 설정되어 있다면 콜라이더(오브젝트) 크기를 키움 (충격파 연출)
        if (expandSpeed > 0f)
        {
            transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[보스 특수 공격] 플레이어 적중! 데미지: {damage}");

            // 투사체도 플레이어의 Controller를 가져와서 데미지를 줍니다!
            PlayerController hitPlayer = other.GetComponent<PlayerController>();
            if (hitPlayer != null)
            {
                hitPlayer.TakeDamage(damage);
            }

            // 타격 시 파괴 옵션이 켜져 있을 때만 파괴 (장판기나 관통기는 파괴되지 않음)
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}