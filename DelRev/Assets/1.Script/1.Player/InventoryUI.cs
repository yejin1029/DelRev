using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
  [Header("UI 슬롯 설정")]
  public List<Image> slotImages;

  private Inventory inventory;

  void Start()
  {
    inventory = FindObjectOfType<Inventory>();
    if (inventory == null)
    {
      Debug.LogError("Inventory 컴포넌트를 찾을 수 없습니다.");
      return;
    }

    UpdateInventoryUI();
    UpdateSlotHighlight(inventory.GetCurrentIndex());
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.ChangeSelectedSlot(0);
    if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.ChangeSelectedSlot(1);
    if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.ChangeSelectedSlot(2);
    if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.ChangeSelectedSlot(3);

    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (scroll > 0f)
    {
      int nextIndex = (inventory.GetCurrentIndex() + 1) % inventory.GetInventorySize();
      inventory.ChangeSelectedSlot(nextIndex);
    }
    else if (scroll < 0f)
    {
      int prevIndex = (inventory.GetCurrentIndex() - 1 + inventory.GetInventorySize()) % inventory.GetInventorySize();
      inventory.ChangeSelectedSlot(prevIndex);
    }

    UpdateInventoryUI();
    UpdateSlotHighlight(inventory.GetCurrentIndex());
  }

  public void UpdateInventoryUI()
  {
    var items = inventory.GetInventoryItems();
    int count = Mathf.Min(slotImages.Count, items.Count);

    for (int i = 0; i < count; i++)
    {
      if (slotImages[i] == null) continue;

      if (items[i] != null)
      {
        slotImages[i].sprite = items[i].itemImage;
        slotImages[i].color = Color.white;
      }
      else
      {
        slotImages[i].sprite = null;
        slotImages[i].color = new Color(1, 1, 1, 0);
      }
    }

    // 남은 슬롯은 비우기
    for (int i = count; i < slotImages.Count; i++)
    {
      if (slotImages[i] == null) continue;

      slotImages[i].sprite = null;
      slotImages[i].color = new Color(1, 1, 1, 0);
    }
  }

  public void UpdateSlotHighlight(int currentIndex)
  {
    if (currentIndex < 0 || currentIndex >= slotImages.Count)
      return;

    for (int i = 0; i < slotImages.Count; i++)
    {
      if (slotImages[i] == null || slotImages[i].transform == null || slotImages[i].transform.parent == null)
        continue;

      Image parent = slotImages[i].transform.parent.GetComponent<Image>();
      if (parent != null)
      {
        parent.color = (i == currentIndex) ? Color.yellow : Color.white;
      }
    }
  }
}
