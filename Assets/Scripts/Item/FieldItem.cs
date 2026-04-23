using System.Collections.Generic;
using UnityEngine;
using TMPro;

// ★ 핵심: 아이템 데이터와 수량을 하나로 묶어주는 구조체
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
        // 1. 플레이어가 근처에 있고, 내용물이 1개라도 있어야 함
        if (!isPlayerNearby || cameraTransform == null || lootItems.Count == 0) return;

        Vector3 dirToItem = (transform.position - cameraTransform.position).normalized;
        float angle = Vector3.Angle(cameraTransform.forward, dirToItem);

        if (angle <= viewAngle)
        {
            if (interactionPrompt != null && !interactionPrompt.activeSelf)
                interactionPrompt.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                PickUpAllItems();
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
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }

    private void PickUpAllItems()
    {
        // ★ 핵심: 리스트를 순회하며 인벤토리에 전부 때려 넣습니다!
        foreach (LootItem loot in lootItems)
        {
            if (loot.itemData != null && loot.amount > 0)
            {
                // 1. 인벤토리에 넣기
                InventoryManager.Instance.AddItem(loot.itemData, loot.amount);
                Debug.Log($"{loot.itemData.itemName}을(를) {loot.amount}개 획득!");

                // ==================================================
                // ★ 누락되었던 핵심 코드: 매니저에게 알림창 생성을 요청합니다!
                // ==================================================
                if (AcquisitionLogManager.Instance != null)
                {
                    AcquisitionLogManager.Instance.ShowLog(loot.itemData, loot.amount);
                }
            }
        }

        // 다 먹었으면 필드에서 파괴
        Destroy(gameObject);
    }
}