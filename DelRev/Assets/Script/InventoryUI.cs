using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryUI : MonoBehaviour
{

  [Header("UI 슬롯 연결")]
  public List<Image> slotImages; // 인벤토리 슬롯 UI들 (드래그로 할당)

  private List<Item> inventory = new List<Item>();
  private List<GameObject> inventoryObjects = new List<GameObject>();
  private int inventorySize = 4;
  private int currentIndex = 0; // 현재 선택된 인벤토리 슬롯

  void Start()
  {
    for (int i = 0; i < inventorySize; i++)
    {
      inventory.Add(null);
      inventoryObjects.Add(null);
    }

    UpdateInventoryUI();
    UpdateSlotHighlight();
  }

  void Update()
  {
    // E로 아이템 줍기
    if (Input.GetKeyDown(KeyCode.E))
    {
      TryPickUpItem();
    }

    // G로 아이템 줍기
    if (Input.GetKeyDown(KeyCode.G))
    {
      DropItem();
    }

    // 1,2,3,4 키로 슬롯 변경
    if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSelectedSlot(0);
    if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSelectedSlot(1);
    if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeSelectedSlot(2);
    if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeSelectedSlot(3);

    // 마우스 휠로 슬롯 변경
    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (scroll > 0f)
    {
      int nextIndex = (currentIndex + 1) % inventorySize;
      ChangeSelectedSlot(nextIndex);
    }
    else if (scroll < 0f)
    {
      int prevIndex = (currentIndex - 1 + inventorySize) % inventorySize;
      ChangeSelectedSlot(prevIndex);
    }
  }

  // 아이템 줍기
  void TryPickUpItem()
  {
    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f);
    GameObject closestObject = null;
    float closestDistance = float.MaxValue;

    foreach (var hitCollider in hitColliders)
    {
      float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

      if (distance < closestDistance && hitCollider.GetComponent<Item>() != null)
      {
        closestDistance = distance;
        closestObject = hitCollider.gameObject;
      }
    }

    if (closestObject != null)
    {
      Item item = closestObject.GetComponent<Item>();

      if (inventory.Where(obj => obj != null).Count() < inventorySize)
      {
        if (inventory[currentIndex] == null)
        {
          inventory[currentIndex] = item;
          inventoryObjects[currentIndex] = closestObject;
        }
        else
        {
          int nextIndex = inventory.FindIndex(i => i == null);
          if (nextIndex != -1)
          {
            inventory[nextIndex] = item;
            inventoryObjects[nextIndex] = closestObject;
          }
        }

        closestObject.SetActive(false); // 주운 아이템은 비활성화
        UpdateInventoryUI();
      }
      else
      {
        Debug.Log("Inventory is full!");
      }
    }
  }

  // 아이템 버리기
  void DropItem()
  {
    if (inventory.Count > 0 && currentIndex < inventory.Count && inventory[currentIndex] != null)
    {
      Item droppedItem = inventory[currentIndex];
      GameObject droppedObject = inventoryObjects[currentIndex];

      inventory[currentIndex] = null;
      inventoryObjects[currentIndex] = null;

      droppedObject.transform.position = transform.position + transform.forward * 2f;
      droppedObject.SetActive(true);

      UpdateInventoryUI();
    }
    else
    {
      Debug.Log("No item to drop in the selected slot!");
    }
  }

  // 슬롯 변경
  void ChangeSelectedSlot(int slotIndex)
  {
    if (slotIndex < 0 || slotIndex >= inventorySize)
    {
      Debug.Log("Invalid slot index!");
      return;
    }

    currentIndex = slotIndex;

    if (inventory[currentIndex] != null)
    {
      Debug.Log($"Selected slot {currentIndex + 1}. Item: {inventory[currentIndex].itemName}");
    }
    else
    {
      Debug.Log($"Selected slot {currentIndex + 1}. No item.");
    }

    UpdateSlotHighlight();
  }

  // UI 업데이트
  void UpdateInventoryUI()
  {
    for (int i = 0; i < inventorySize; i++)
    {
      if (inventory[i] != null)
      {
        slotImages[i].sprite = inventory[i].itemImage;
        slotImages[i].color = Color.white;
      }
      else
      {
        slotImages[i].sprite = null;
        slotImages[i].color = new Color(1, 1, 1, 0); // 투명
      }
    }
  }

  // 선택하고 있는 인벤토리 테두리 색 변경
  void UpdateSlotHighlight()
  {
    for (int i = 0; i < slotImages.Count; i++)
    {
      Image parent = slotImages[i].transform.parent.GetComponent<Image>();
      if (parent != null)
      {
        parent.color = (i == currentIndex) ? Color.yellow : Color.white;
      }
    }
  }
}
