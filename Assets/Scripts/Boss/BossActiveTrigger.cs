using UnityEngine;

public class BossActiveTrigger : MonoBehaviour
{
    [Header("연결할 보스")]
    // 이 방의 주인인 보스를 연결해 줍니다.
    public Skeleton_Guard_Controller bossController;

    [Header("보스방 입구 막기 (옵션)")]
    // 보스방에 들어오면 도망 못 가게 안개벽을 칠 수 있습니다.
    public GameObject fogWall;

    private void OnTriggerEnter(Collider other)
    {
        // 밟은 게 플레이어라면?
        if (other.CompareTag("Player"))
        {
            if (bossController != null)
            {
                // 1. 보스를 깨웁니다!
                bossController.WakeUpBoss();
            }

            if (fogWall != null)
            {
                // 2. 도망 못 가게 입구를 막아버립니다!
                fogWall.SetActive(true);
            }

            // 3. 트리거는 할 일을 다 했으니 자신을 지워버립니다. (중복 발동 방지)
            Destroy(gameObject);
        }
    }
}
