using System;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    //  1. 빈칸 개념이 없는 마스터 명부 (내가 가진 모든 아이템)
    [Header("Master Inventory")]
    public List<ItemInstance> allItems = new List<ItemInstance>();

    public event Action OnInventoryUpdated;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    /// <summary>
    /// 아이템 획득 로직
    /// </summary>
    public void AddItem(ItemDataBase data, int amount = 1)
    {
        // 포션/재료 등 스택 가능한 아이템인지 확인
        if (data.maxStackAmount > 1)
        {
            // LINQ를 사용해 이미 가지고 있는 동일 아이템 검색
            ItemInstance existingItem = allItems.FirstOrDefault(i => i.itemID == data.itemID);

            if (existingItem != null)
            {
                existingItem.amount += amount;
                // 엘든링은 최대 소지수를 초과하면 창고(Storage)로 보냅니다. (여기선 생략)
                OnInventoryUpdated?.Invoke();
                return;
            }
        }

        // 무기/방어구이거나, 스택 아이템인데 처음 먹는 경우 무조건 새 인스턴스 생성
        ItemInstance newItem = new ItemInstance(data, amount);
        allItems.Add(newItem);

        OnInventoryUpdated?.Invoke();
    }

    /// <summary>
    /// ★ 엘든링 시스템의 핵심: 카테고리별로 아이템 걸러서 뽑아주기
    /// 장비창에서 '무기 슬롯'을 클릭했을 때 이 함수를 호출합니다.
    /// </summary>
    public List<ItemInstance> GetItemsByType(ItemType targetType)
    {
        // allItems 리스트 중에서 itemType이 targetType과 일치하는 것만 리스트로 반환
        return allItems.Where(item => item.itemData.itemType == targetType).ToList();
    }

    /// <summary>
    /// 장신구(반지)처럼 카테고리가 세분화될 경우를 위한 고급 필터링
    /// </summary>
    public List<ItemInstance> GetItemsByCategory(params ItemType[] allowedTypes)
    {
        return allItems.Where(item => allowedTypes.Contains(item.itemData.itemType)).ToList();
    }

    /// <summary>
    /// 아이템 제거 로직
    /// </summary>
    public void RemoveItem(ItemInstance itemToRemove, int amount = 1)
    {
        if (!allItems.Contains(itemToRemove)) return;

        itemToRemove.amount -= amount;

        if (itemToRemove.amount <= 0)
        {
            allItems.Remove(itemToRemove); // 개수가 0이 되면 명부에서 완전히 삭제
        }

        OnInventoryUpdated?.Invoke();
    }
}