using UnityEngine;
using TMPro;

public class MoneyUiManager : MonoBehaviour
{
    [Header("연결 요소")]
    public PlayerStats playerStats;
    public TextMeshProUGUI moneyText; 

    private void Start()
    {
        if (playerStats != null)
        {
            // 방송국 구독
            playerStats.OnMoneyChanged += UpdateMoneyUI;

            // 게임 시작할 때 현재 잔액으로 초기화
            UpdateMoneyUI(playerStats.currentMoney);
        }
    }

    private void UpdateMoneyUI(int newAmount)
    {
        if (moneyText != null)
        {
            // 숫자에 콤마(,)를 찍어서 예쁘게 보여줍니다. (예: 1000 -> 1,000)
            moneyText.text = newAmount.ToString("N0");
        }
    }

    private void OnDestroy()
    {
        // 구독 해제
        if (playerStats != null)
        {
            playerStats.OnMoneyChanged -= UpdateMoneyUI;
        }
    }
}