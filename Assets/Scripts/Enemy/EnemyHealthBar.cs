using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("연결 요소")]
    public EnemyStats enemyStats;
    public Image healthFillImage;
    public GameObject visualParent; // Canvas 내부의 UI 요소들을 담고 있는 부모 객체

    [Header("가시성 설정")]
    public float showDuration = 5f; // 맞았을 때 보여줄 시간
    private float visibilityTimer;

    private PlayerController player; // 플레이어의 락온 상태를 확인하기 위함
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        player = FindObjectOfType<PlayerController>(); // 플레이어 참조 (Singleton이 있다면 인스턴스 사용 권장)

        if (enemyStats != null)
        {
            enemyStats.OnHpChanged += UpdateHealthBar;
            enemyStats.OnTakeDamage += TriggerShow; // 데미지를 입었을 때 타이머 시작

            UpdateHealthBar(enemyStats.currentHealth, enemyStats.maxHealth);
        }

        // 시작할 때는 숨김 처리
        if (visualParent != null) visualParent.SetActive(false);
    }

    private void Update()
    {
        // 1. 빌보드 (카메라 바라보기)
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }

        // 2. 가시성 판단 로직
        HandleVisibility();
    }

    private void HandleVisibility()
    {
        // 타이머 감소
        if (visibilityTimer > 0) visibilityTimer -= Time.deltaTime;

        // 락온 여부 확인 (내 lockOnPoint가 플레이어의 현재 타겟인지 확인)
        bool isBeingLockedOn = false;
        if (player != null && player.isLockedOn && player.currentTarget != null)
        {
            // 플레이어가 잡고 있는 타겟이 내 자식(lockOnPoint)이거나 나 자신인지 체크
            if (player.currentTarget == enemyStats.transform || player.currentTarget.IsChildOf(enemyStats.transform))
            {
                isBeingLockedOn = true;
            }
        }

        // 조건: 타이머가 남았거나, 락온 중일 때만 활성화
        bool shouldShow = (visibilityTimer > 0) || isBeingLockedOn;

        if (visualParent != null)
        {
            if (visualParent.activeSelf != shouldShow)
            {
                visualParent.SetActive(shouldShow);
            }
        }
    }

    private void TriggerShow()
    {
        // 맞으면 타이머를 다시 채움
        visibilityTimer = showDuration;
    }

    private void UpdateHealthBar(float currentHp, float maxHp)
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHp / maxHp;
        }

        if (currentHp <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (enemyStats != null)
        {
            enemyStats.OnHpChanged -= UpdateHealthBar;
            enemyStats.OnTakeDamage -= TriggerShow;
        }
    }
}