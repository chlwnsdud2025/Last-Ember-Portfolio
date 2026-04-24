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

    [Header("Selection Panel (Highlight)")]
    public RectTransform selectionPanel; // 움직일 하이라이트 패널
    public RectTransform[] rowPositions; // 패널이 이동할 4개의 위치 (각 스탯 줄)

    private PlayerStats playerStats;

    // 장바구니 배열 (0:체력, 1:지구력, 2:근력, 3:기량)
    private int[] pendingStats = new int[4];
    private int selectedIndex = 0; // 현재 선택된 스탯 줄 번호

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
        // 레벨업 창이 꺼져있으면 키보드 입력을 무시합니다.
        if (!levelUpPanel.activeSelf) return;

        // 1. 위로 이동 (W 또는 위쪽 방향키)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            int newIndex = selectedIndex - 1;
            if (newIndex < 0) newIndex = 3; // 맨 위에서 한 번 더 올리면 맨 아래(기량)로 이동
            SelectStatRow(newIndex);
        }
        // 2. 아래로 이동 (S 또는 아래쪽 방향키)
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            int newIndex = selectedIndex + 1;
            if (newIndex > 3) newIndex = 0; // 맨 아래에서 한 번 더 내리면 맨 위(체력)로 이동
            SelectStatRow(newIndex);
        }

        // 3. 스탯 감소 (A 또는 왼쪽 방향키)
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnClickDecreaseSelected();
        }
        // 4. 스탯 증가 (D 또는 오른쪽 방향키)
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnClickIncreaseSelected();
        }
    }

    public void OpenLevelUpPanel()
    {
        // 장바구니 초기화
        for (int i = 0; i < pendingStats.Length; i++) pendingStats[i] = 0;

        UIManager.Instance.OpenPanel(levelUpPanel);
        UpdateUI(); SelectStatRow(0); // 처음 열면 무조건 맨 윗줄(체력) 선택
        UpdateUI();
    }

    public void CloseLevelUpPanel()
    {
        UIManager.Instance.ClosePanel(levelUpPanel);
    }

    // 선택 패널 이동 로직
    public void SelectStatRow(int index)
    {
        selectedIndex = index;

        // 하이라이트 패널을 선택한 줄의 위치로 부드럽게(또는 즉시) 이동
        if (selectionPanel != null && rowPositions.Length > index)
        {
            selectionPanel.position = rowPositions[index].position;
        }
    }

    // ==========================================
    // ★ 선택 패널에 달린 + / - 버튼 작동 로직
    // ==========================================
    public void OnClickIncreaseSelected()
    {
        int totalPending = GetTotalPendingLevels();
        int nextCost = playerStats.GetCostForLevel(playerStats.level + totalPending);

        // 돈이 충분할 때만 증가
        if (playerStats.currentMoney - CalculateTotalPendingCost() >= nextCost)
        {
            pendingStats[selectedIndex]++;
            UpdateUI();
        }
    }

    public void OnClickDecreaseSelected()
    {
        // 0보다 작아질 수는 없음 (기본 스탯 밑으로 깎기 불가)
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
    // ★ 화면 갱신 (파란색 색상 적용 등)
    // ==========================================
    public void UpdateUI()
    {
        int totalPending = GetTotalPendingLevels();
        int totalCost = CalculateTotalPendingCost();

        int projectedLevel = playerStats.level + totalPending;
        int projectedMoney = playerStats.currentMoney - totalCost;
        int nextLevelCost = playerStats.GetCostForLevel(projectedLevel);

        // 상단 정보 (Current)
        levelCurrentText.text = playerStats.level.ToString();
        moneyCurrentText.text = playerStats.currentMoney.ToString();
        costText.text = nextLevelCost.ToString();

        // 상단 정보 (After - 변화가 있으면 파란색, 돈은 깎이니 빨간색 느낌도 좋지만 통일성을 위해 파란색)
        string blueColor = "<color=#5B9BD5>"; // 엘든링 느낌의 파란색
        string endColor = "</color>";

        levelAfterText.text = totalPending > 0 ? $"{blueColor}{projectedLevel}{endColor}" : projectedLevel.ToString();
        moneyAfterText.text = totalPending > 0 ? $"{blueColor}{projectedMoney}{endColor}" : projectedMoney.ToString();

        // 베이스 스탯 배열
        int[] baseStats = { playerStats.vigor, playerStats.endurance, playerStats.strength, playerStats.dexterity };

        for (int i = 0; i < 4; i++)
        {
            currentStatTexts[i].text = baseStats[i].ToString();

            int afterStat = baseStats[i] + pendingStats[i];

            // 펜딩 수치가 1 이상이면 파란색으로 AFTER 표시!
            if (pendingStats[i] > 0)
            {
                afterStatTexts[i].text = $"{blueColor}{afterStat}{endColor}";
            }
            else
            {
                afterStatTexts[i].text = afterStat.ToString();
            }
        }
    }

    public void ConfirmLevelUp()
    {
        int totalPending = GetTotalPendingLevels();
        if (totalPending <= 0) return;

        playerStats.ApplyLevelUp(pendingStats[0], pendingStats[1], pendingStats[2], pendingStats[3], CalculateTotalPendingCost());

        // 초기화 및 갱신
        for (int i = 0; i < pendingStats.Length; i++) pendingStats[i] = 0;
        UpdateUI();
    }
}