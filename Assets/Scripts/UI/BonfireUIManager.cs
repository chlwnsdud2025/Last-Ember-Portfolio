using UnityEngine;

public class BonfireUIManager : MonoBehaviour
{
    public static BonfireUIManager Instance { get; private set; }

    [Header("Resting HUD")]
    public GameObject restingMenuCanvas; // 버튼들이 담긴 전체 부모 오브젝트

    private void Awake()
    {
        if (Instance == null) Instance = this;
        restingMenuCanvas.SetActive(false);
    }

    // 화톳불에 앉거나 일어날 때 호출
    public void ToggleRestingHUD(bool isShowing)
    {
        restingMenuCanvas.SetActive(isShowing);

        // 메뉴를 닫을 때는 열려있던 모든 세부 패널(레벨업 등)도 닫아줌
        if (!isShowing)
        {
            LevelUpUIManager.Instance.CloseLevelUpPanel();            
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCursorState();
        }
    }

    // [버튼 연결용] 레벨업 메뉴 클릭 시
    public void OnClickLevelUpMenu()
    {
        LevelUpUIManager.Instance.OpenLevelUpPanel();
    }

    // [버튼 연결용] 나가기(일어나기) 클릭 시
    public void OnClickLeave()
    {
        // 플레이어에게 E키를 눌렀을 때와 동일한 효과를 줍니다.
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (player.stateMachine.CurrentState is RestState restState)
        {
            // RestState에 우리가 만들어둔 로직을 타게 함 (isResting = false)
            player.animator.SetBool("isResting", false);
            player.animator.CrossFadeInFixedTime("Bonfire.End", 0.1f, 0);
        }
    }
}