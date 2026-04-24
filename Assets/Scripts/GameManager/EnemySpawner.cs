using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab; // 생성할 몬스터 프리팹 (예: 해골병사)

    private GameObject spawnedEnemy;

    private void Start()
    {
        // 1. 게임 시작 시 몬스터를 스폰합니다.
        SpawnEnemy();

        // 2. 화톳불 휴식 이벤트에 귀를 기울입니다(구독).
        Bonfire.OnBonfireRest += ResetEnemy;
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때는 반드시 이벤트 구독을 취소해야 에러가 안 납니다.
        Bonfire.OnBonfireRest -= ResetEnemy;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            // 스포너의 위치와 회전값을 그대로 적용하여 적 생성
            spawnedEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        }
    }

    private void ResetEnemy()
    {
        // 1. 기존 몬스터가 살아있거나 시체로 남아있다면 삭제
        if (spawnedEnemy != null)
        {
            Destroy(spawnedEnemy);
        }

        // 2. 새롭고 건강한 몬스터로 다시 스폰
        SpawnEnemy();
    }
}