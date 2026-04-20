using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트(이름)를 위해 TextMeshPro 사용

public class BossUIManager : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public GameObject bossUIContainer;  // 체력바와 이름표를 모두 담고 있는 부모 객체
    public Image healthFillImage;       // 체력을 채울 빨간색 이미지
    public TextMeshProUGUI bossNameText;// 보스 이름 텍스트

    [Header("데이터 연결")]
    public EnemyStats bossStats;        // 보스의 체력 스탯 스크립트
    public string bossName = "스켈레톤 가드"; // 화면에 띄울 보스 이름

    private void Start()
    {
        // 1. 게임 시작 시 보스 UI는 무조건 숨겨둡니다.
        if (bossUIContainer != null) bossUIContainer.SetActive(false);

        // 2. 보스의 체력 방송국(OnHpChanged)을 구독합니다!
        if (bossStats != null)
        {
            bossStats.OnHpChanged += UpdateBossHealthBar;
        }
    }

    //  보스방에 입장했을 때 외부에서 호출할 함수
    public void ActivateBossUI()
    {
        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(true);
            if (bossNameText != null) bossNameText.text = bossName;

            // UI를 켜자마자 현재 체력으로 꽉 채워줍니다.
            UpdateBossHealthBar(bossStats.currentHealth, bossStats.maxHealth);
        }
    }

    //  보스가 데미지를 입을 때마다 자동으로 실행되는 함수
    private void UpdateBossHealthBar(float currentHp, float maxHp)
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHp / maxHp;
        }

        // 보스가 죽으면 UI를 다시 숨깁니다.
        if (currentHp <= 0 && bossUIContainer != null)
        {
            bossUIContainer.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // 에러 방지용 구독 해제
        if (bossStats != null)
        {
            bossStats.OnHpChanged -= UpdateBossHealthBar;
        }
    }
}