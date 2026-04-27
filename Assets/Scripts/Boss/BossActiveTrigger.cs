using UnityEngine;

public class BossActiveTrigger : MonoBehaviour
{
    [Header("연결할 보스")]
    public Skeleton_Guard_Controller bossController;

    [Header("보스방 입구 막기 (옵션)")]
    public GameObject fogWall;

    private void Start()
    {
        // 화톳불 리셋 이벤트를 구독하여 트리거와 안개벽을 초기화합니다.
        Bonfire.OnBonfireRest += ResetTrigger;
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 에러 방지를 위해 구독 해제
        Bonfire.OnBonfireRest -= ResetTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (bossController != null)
            {
                bossController.WakeUpBoss();
            }

            if (fogWall != null)
            {
                fogWall.SetActive(true);
            }

            //  Destroy 대신 콜라이더(판정)만 꺼서 재사용할 수 있게 만듭니다.
            GetComponent<Collider>().enabled = false;
        }
    }

    // 화톳불에서 쉬거나 부활할 때 실행될 함수
    private void ResetTrigger()
    {
        // 1. 트리거 판정 다시 켜기
        GetComponent<Collider>().enabled = true;

        // 2. 안개벽 끄기
        if (fogWall != null)
        {
            fogWall.SetActive(false);
        }
        Debug.Log("보스방 트리거 & 안개벽 리셋 완료!");
    }
}