using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelUpUIManager : MonoBehaviour
{
    public static LevelUpUIManager Instance { get; private set; }

    public GameObject levelUpPanel;

    [Header("Top Status")]
    public TextMeshProUGUI levelCurrentText;
    public TextMeshProUGUI levelAfterText;
    public TextMeshProUGUI moneyCurrentText;
    public TextMeshProUGUI moneyAfterText;
    public TextMeshProUGUI costText;

    [Header("Stats (0:체력, 1:지구력, 2:근력, 3:기량)")]
    public TextMeshProUGUI[] currentStatTexts;
    public TextMeshProUGUI[] afterStatTexts;

    
    // 우측 패널 (상세 데이터) 텍스트 연결 변수들
    
    [Header("Right Panel: Derived Stats (Current)")]
    public TextMeshProUGUI hpCurrentText;
    public TextMeshProUGUI staminaCurrentText;
    public TextMeshProUGUI staminaRegenCurrentText;
    public TextMeshProUGUI[] weaponCurrentTexts; // 무기 1~4 현재 데미지

    [Header("Right Panel: Derived Stats (After)")]
    public TextMeshProUGUI hpAfterText;
    public TextMeshProUGUI staminaAfterText;
    public TextMeshProUGUI staminaRegenAfterText;
    public TextMeshProUGUI[] weaponAfterTexts;   // 무기 1~4 변경될 데미지

    [Header("Selection Panel (Highlight)")]
    public RectTransform selectionPanel;
    public RectTransform[] rowPositions;

    private PlayerStats playerStats;

    // 장바구니 배열 (0:체력, 1:지구력, 2:근력, 3:기량)
    private int[] pendingStats = new int[4];
    private int selectedIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        levelUpPanel.SetActive(false);
    }

    private void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!levelUpPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            int newIndex = selectedIndex - 1;
            if (newIndex < 0) newIndex = 3;
            SelectStatRow(newIndex);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            int newIndex = selectedIndex + 1;
            if (newIndex > 3) newIndex = 0;
            SelectStatRow(newIndex);
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnClickDecreaseSelected();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnClickIncreaseSelected();
        }
    }

    public void OpenLevelUpPanel()
    {
        for (int i = 0; i < pendingStats.Length; i++) pendingStats[i] = 0;

        UIManager.Instance.OpenPanel(levelUpPanel);
        SelectStatRow(0);
        UpdateUI();
    }

    public void CloseLevelUpPanel()
    {
        UIManager.Instance.ClosePanel(levelUpPanel);
    }

    public void SelectStatRow(int index)
    {
        selectedIndex = index;
        if (selectionPanel != null && rowPositions.Length > index)
        {
            selectionPanel.position = rowPositions[index].position;
        }
    }

    public void OnClickIncreaseSelected()
    {
        int totalPending = GetTotalPendingLevels();
        int nextCost = playerStats.GetCostForLevel(playerStats.level + totalPending);

        if (playerStats.currentMoney - CalculateTotalPendingCost() >= nextCost)
        {
            pendingStats[selectedIndex]++;
            UpdateUI();
        }
    }

    public void OnClickDecreaseSelected()
    {
        if (pendingStats[selectedIndex] > 0)
        {
            pendingStats[selectedIndex]--;
            UpdateUI();
        }
    }

    private int GetTotalPendingLevels()
    {
        int sum = 0;
        foreach (int val in pendingStats) sum += val;
        return sum;
    }

    private int CalculateTotalPendingCost()
    {
        int cost = 0;
        int totalPending = GetTotalPendingLevels();
        for (int i = 0; i < totalPending; i++)
        {
            cost += playerStats.GetCostForLevel(playerStats.level + i);
        }
        return cost;
    }

    // ==========================================
    //  우측 패널 상세 스탯 계산 및 렌더링 로직 추가
    // ==========================================
    public void UpdateUI()
    {
        string blueColor = "<color=#5B9BD5>";
        string endColor = "</color>";

        int totalPending = GetTotalPendingLevels();
        int totalCost = CalculateTotalPendingCost();

        int projectedLevel = playerStats.level + totalPending;
        int projectedMoney = playerStats.currentMoney - totalCost;
        int nextLevelCost = playerStats.GetCostForLevel(projectedLevel);

        // 1. 좌측 상단 정보
        levelCurrentText.text = playerStats.level.ToString();
        moneyCurrentText.text = playerStats.currentMoney.ToString();
        costText.text = nextLevelCost.ToString();

        levelAfterText.text = totalPending > 0 ? $"{blueColor}{projectedLevel}{endColor}" : projectedLevel.ToString();
        moneyAfterText.text = totalPending > 0 ? $"{blueColor}{projectedMoney}{endColor}" : projectedMoney.ToString();

        // 2. 좌측 하단 메인 스탯
        int[] baseStats = { playerStats.vigor, playerStats.endurance, playerStats.strength, playerStats.dexterity };
        for (int i = 0; i < 4; i++)
        {
            currentStatTexts[i].text = baseStats[i].ToString();
            int afterStat = baseStats[i] + pendingStats[i];

            afterStatTexts[i].text = pendingStats[i] > 0 ? $"{blueColor}{afterStat}{endColor}" : afterStat.ToString();
        }

        // ------------------------------------------
        // 3. 우측 패널 상세 데이터 업데이트 (HP, 스태미나)
        // ------------------------------------------
        // PlayerStats.cs의 공식에 따라 예상 HP/Stamina를 계산[cite: 10]
        float currentMaxHp = playerStats.maxHp;
        float projectedMaxHp = 40f + ((playerStats.vigor + pendingStats[0]) * 10f); // 체력 스탯 연동

        float currentMaxStamina = playerStats.maxStamina;
        float projectedMaxStamina = 30f + ((playerStats.endurance + pendingStats[1]) * 10f); // 지구력 스탯 연동

        if (hpCurrentText != null) hpCurrentText.text = Mathf.RoundToInt(currentMaxHp).ToString();
        if (hpAfterText != null) hpAfterText.text = pendingStats[0] > 0 ? $"{blueColor}{Mathf.RoundToInt(projectedMaxHp)}{endColor}" : Mathf.RoundToInt(projectedMaxHp).ToString();

        if (staminaCurrentText != null) staminaCurrentText.text = Mathf.RoundToInt(currentMaxStamina).ToString();
        if (staminaAfterText != null) staminaAfterText.text = pendingStats[1] > 0 ? $"{blueColor}{Mathf.RoundToInt(projectedMaxStamina)}{endColor}" : Mathf.RoundToInt(projectedMaxStamina).ToString();

        // 스태미나 재생량은 고정값이므로 동일하게 표기 (나중에 공식을 넣으면 여기도 연동 가능)[cite: 10]
        if (staminaRegenCurrentText != null) staminaRegenCurrentText.text = playerStats.staminaRegenRate.ToString();
        if (staminaRegenAfterText != null) staminaRegenAfterText.text = playerStats.staminaRegenRate.ToString();


        
        // 4. 우측 패널 장착 무기 데미지 업데이트
        int projectedStr = playerStats.strength + pendingStats[2];
        int projectedDex = playerStats.dexterity + pendingStats[3];

        if (EquipmentManager.Instance != null && weaponCurrentTexts != null && weaponAfterTexts != null)
        {
            for (int i = 0; i < 4; i++)
            {
                // UI 텍스트 배열 길이를 초과하지 않도록 안전장치
                if (i >= weaponCurrentTexts.Length || i >= weaponAfterTexts.Length) break;

                // 해당 슬롯에 무기가 장착되어 있는지 확인
                if (EquipmentManager.Instance.currentWeapons.Length > i && EquipmentManager.Instance.currentWeapons[i] != null)
                {
                    ItemInstance weapon = EquipmentManager.Instance.currentWeapons[i];

                    // 현재 데미지와 예상 데미지 계산
                    float currentDmg = weapon.GetCalculatedDamage(playerStats.strength, playerStats.dexterity);
                    float projectedDmg = weapon.GetCalculatedDamage(projectedStr, projectedDex);

                    weaponCurrentTexts[i].text = Mathf.RoundToInt(currentDmg).ToString();

                    // 근력(pendingStats[2])이나 기량(pendingStats[3])이 올라서 데미지가 증가했다면 파란색!
                    if (projectedDmg > currentDmg)
                    {
                        weaponAfterTexts[i].text = $"{blueColor}{Mathf.RoundToInt(projectedDmg)}{endColor}";
                    }
                    else
                    {
                        weaponAfterTexts[i].text = Mathf.RoundToInt(projectedDmg).ToString();
                    }
                }
                else
                {
                    // 무기가 안 껴져 있으면 "-" 로 표기
                    weaponCurrentTexts[i].text = "-";
                    weaponAfterTexts[i].text = "-";
                }
            }
        }
    }

    public void ConfirmLevelUp()
    {
        int totalPending = GetTotalPendingLevels();
        if (totalPending <= 0) return;

        // ★[cite: 10] 제거됨
        playerStats.ApplyLevelUp(pendingStats[0], pendingStats[1], pendingStats[2], pendingStats[3], CalculateTotalPendingCost());

        // 초기화 및 갱신
        for (int i = 0; i < pendingStats.Length; i++) pendingStats[i] = 0;
        UpdateUI();
    }
}