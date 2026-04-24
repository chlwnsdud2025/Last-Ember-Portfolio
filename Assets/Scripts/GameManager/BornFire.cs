using UnityEngine;
using System;
using TMPro;

public class Bonfire : MonoBehaviour
{
    //핵심: 전역 이벤트 (화톳불에서 쉴 때 발생하는 방송)
    public static event Action OnBonfireRest;

    [Header("Interaction Settings")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    [Range(10f, 90f)] public float viewAngle = 60f;

    private bool isPlayerNearby = false;
    private Transform cameraTransform;
    private PlayerStats playerStats;

    private void Start()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (promptText != null) promptText.text = "Press E to Rest";
        if (Camera.main != null) cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (!isPlayerNearby || cameraTransform == null) return;

        Vector3 dirToBonfire = (transform.position - cameraTransform.position).normalized;
        float angle = Vector3.Angle(cameraTransform.forward, dirToBonfire);

        if (angle <= viewAngle)
        {
            if (interactionPrompt != null && !interactionPrompt.activeSelf)
                interactionPrompt.SetActive(true);

            // E 키를 눌러 휴식
            if (Input.GetKeyDown(KeyCode.E))
            {
                Rest();
            }
        }
        else
        {
            if (interactionPrompt != null && interactionPrompt.activeSelf)
                interactionPrompt.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // 플레이어의 스탯 스크립트를 캐싱해 둡니다.
            if (playerStats == null) playerStats = other.GetComponent<PlayerStats>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }

    private void Rest()
    {
        if (playerStats != null)
        {
            // 1. 플레이어의 컨트롤러(상태 머신)를 가져옵니다.
            PlayerController playerController = playerStats.GetComponent<PlayerController>();

            // 플레이어가 이미 휴식 상태가 아닐 때만 실행
            if (playerController != null && playerController.stateMachine.CurrentState != playerController.restState)
            {
                // \상태 머신을 RestState로 전환
                playerController.stateMachine.ChangeState(playerController.restState);

                // 2. 플레이어 회복 (체력, 스태미나 100%)
                playerStats.RestAtBonfire();

                // 3. 월드 리셋 이벤트 발생 (스포너 작동)
                OnBonfireRest?.Invoke();
            }
        }
    }
}