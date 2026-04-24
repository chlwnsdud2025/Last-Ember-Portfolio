using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 아이템 데이터와 수량을 하나로 묶어주는 구조체
[System.Serializable]
public struct LootItem
{
    public ItemDataBase itemData;
    public int amount;
}

public class FieldItem : MonoBehaviour
{
    [Header("Loot List")]
    // 여러 개의 아이템을 담을 수 있는 리스트!
    public List<LootItem> lootItems = new List<LootItem>();

    [Header("Interaction Settings")]
    public GameObject interactionPrompt;
    [Range(10f, 90f)]
    public float viewAngle = 60f;

    private bool isPlayerNearby = false;
    private Transform cameraTransform;

    private PlayerController playerController;

    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (Camera.main != null) cameraTransform = Camera.main.transform;

    }

    // 몬스터 드랍 시 외부에서 리스트를 통째로 주입할 때 쓰는 함수
    public void Setup(List<LootItem> droppedItems)
    {
        lootItems = droppedItems;
    }

    // 아이템 개수에 따라 UI 텍스트를 똑똑하게 바꿔주는 함수


    private void Update()
    {
        if (!isPlayerNearby || cameraTransform == null || lootItems.Count == 0) return;

        Vector3 dirToItem = (transform.position - cameraTransform.position).normalized;
        float angle = Vector3.Angle(cameraTransform.forward, dirToItem);

        if (angle <= viewAngle)
        {
            if (interactionPrompt != null && !interactionPrompt.activeSelf)
                interactionPrompt.SetActive(true);

            // 현재 플레이어가 보고 있는 아이템으로 등록
            if (playerController != null) playerController.focusedItem = this;
        }
        else
        {
            if (interactionPrompt != null && interactionPrompt.activeSelf)
                interactionPrompt.SetActive(false);

            // 시야에서 벗어나면 등록 해제
            if (playerController != null && playerController.focusedItem == this)
                playerController.focusedItem = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerController = other.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (playerController != null && playerController.focusedItem == this)
                playerController.focusedItem = null;
            playerController = null;

            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }

    public void ExecutePickUp()
    {
        foreach (LootItem loot in lootItems)
        {
            if (loot.itemData != null && loot.amount > 0)
            {
                InventoryManager.Instance.AddItem(loot.itemData, loot.amount);
                if (AcquisitionLogManager.Instance != null)
                    AcquisitionLogManager.Instance.ShowLog(loot.itemData, loot.amount);
            }
        }
        Destroy(gameObject);
    }
}