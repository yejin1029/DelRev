using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Inventory : MonoBehaviour
{
    private List<Item> inventory = new List<Item>();
    private List<GameObject> inventoryObjects = new List<GameObject>();
    private int inventorySize = 4;
    private int currentIndex = 0; // 현재 선택된 인벤토리 슬롯

    public List<Item> GetInventoryItems()
    {
        return inventory;
      }

      public int GetCurrentIndex()
      {
          return currentIndex;
      }

      public int GetInventorySize()
      {
          return inventorySize;
      }

    void Start()
    {
        // 고정 크기의 인벤토리 공간 초기화
        for (int i = 0; i < inventorySize; i++)
        {
            inventory.Add(null);
            inventoryObjects.Add(null);
        }
    }

    void Update()
    {
        // 아이템 줍기 (E키)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupItem();
        }

        // 아이템 버리기 (G키)
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropItem();
        }

        // 인벤토리 슬롯 변경 (1~4 키)
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSelectedSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSelectedSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeSelectedSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeSelectedSlot(3);
    }

    void TryPickupItem()
    {
        // 카메라 중심에서 정면으로 레이 발사
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 4f)) // 줍기 가능한 거리
        {
            GameObject hitObject = hit.collider.gameObject;
            Item item = hitObject.GetComponent<Item>();

            if (item != null)
            {
                // 현재 슬롯이 비어 있어야만 아이템을 저장
                if (inventory[currentIndex] == null)
                {
                    inventory[currentIndex] = item;
                    inventoryObjects[currentIndex] = hitObject;
                    hitObject.SetActive(false);

                    Debug.Log($"Picked up: {item.itemName} to slot {currentIndex + 1}. Inventory: {string.Join(", ", inventory.ConvertAll(i => i?.itemName ?? "Empty"))}");
                }
                else
                {
                    Debug.Log($"Slot {currentIndex + 1} is already occupied!");
                }
            }
        }
    }

    void DropItem()
    {
        if (inventory.Count > 0 && currentIndex < inventory.Count && inventory[currentIndex] != null)
        {
            Item droppedItem = inventory[currentIndex];
            GameObject droppedObject = inventoryObjects[currentIndex];

            inventory[currentIndex] = null;
            inventoryObjects[currentIndex] = null;

            Vector3 dropPosition = transform.position + transform.forward * 2f;
            droppedObject.transform.position = dropPosition;
            droppedObject.SetActive(true);

            Debug.Log($"Dropped: {droppedItem.itemName}. Inventory: {string.Join(", ", inventory.ConvertAll(i => i?.itemName ?? "Empty"))}");
        }
        else
        {
            Debug.Log("No item to drop in the selected slot!");
        }
    }

    public void ChangeSelectedSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
        {
            Debug.Log("Invalid slot index!");
            return;
        }

        currentIndex = slotIndex;

        if (inventory[currentIndex] != null)
        {
            Debug.Log($"Selected slot {currentIndex + 1}. Current item: {inventory[currentIndex].itemName}");
        }
        else
        {
            Debug.Log($"Selected slot {currentIndex + 1}. No item in this slot.");
        }
    }
}
