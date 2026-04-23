using UnityEngine;

public class AcquisitionLogManager : MonoBehaviour
{
    public static AcquisitionLogManager Instance { get; private set; }

    [Header("Settings")]
    public GameObject logPrefab;       // 방금 만든 알림창 프리팹
    public Transform logContainer;     // 알림창들이 차곡차곡 쌓일 부모 패널

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 외부에서 아이템을 먹었을 때 호출하는 함수
    public void ShowLog(ItemDataBase itemData, int amount)
    {
        if (itemData == null || logPrefab == null || logContainer == null) return;

        // 1. 프리팹을 컨테이너 안에 생성
        GameObject logObj = Instantiate(logPrefab, logContainer);

        // 2. 생성된 프리팹의 스크립트를 가져와서 데이터 꽂아주기
        AcquisitionLogUI logUI = logObj.GetComponent<AcquisitionLogUI>();
        if (logUI != null)
        {
            logUI.Setup(itemData, amount);
        }
    }
}