using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [Header("테스트용 아이템 데이터 (인스펙터에서 드래그 앤 드롭)")]
    [Tooltip("여기에 무기 ScriptableObject 2개를 넣어주세요")]
    public ItemDataBase[] testWeapons;

    [Tooltip("여기에 방어구 ScriptableObject 2개를 넣어주세요")]
    public ItemDataBase[] testArmors;

    [Tooltip("여기에 장신구/아티팩트 ScriptableObject 2개를 넣어주세요")]
    public ItemDataBase[] testArtifacts;

    [Tooltip("여기에 포션 등 소모품 ScriptableObject 2개를 넣어주세요")]
    public ItemDataBase[] testConsumables;

    private void Update()
    {
        // 키보드 T 키를 누르면 아이템 지급!
        if (Input.GetKeyDown(KeyCode.T))
        {
            GiveTestItems();
        }
    }

    private void GiveTestItems()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("씬에 InventoryManager가 없습니다! 먼저 배치해주세요.");
            return;
        }

        // 각 카테고리 배열에서 아이템을 꺼내서 매니저에게 전달합니다.
        // 무기, 방어구, 장신구는 1개씩 지급
        AddItemsFromArray(testWeapons, 1);
        AddItemsFromArray(testArmors, 1);
        AddItemsFromArray(testArtifacts, 1);

        // 소모품은 겹치기(Stack) 기능 테스트를 위해 5개씩 지급해봅니다.
        AddItemsFromArray(testConsumables, 5);

        Debug.Log(" 테스트용 아이템 지급 완료! Tab 키를 눌러 인벤토리를 확인하세요.");
    }

    private void AddItemsFromArray(ItemDataBase[] itemArray, int amountToGive)
    {
        if (itemArray == null || itemArray.Length == 0) return;

        // 인스펙터에 등록된 모든 아이템을 제한 없이 지급합니다!
        foreach (ItemDataBase itemData in itemArray)
        {
            if (itemData != null)
            {
                InventoryManager.Instance.AddItem(itemData, amountToGive);
            }
        }
    }
}