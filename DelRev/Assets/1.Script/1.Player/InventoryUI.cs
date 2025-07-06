// InventoryUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("UI 슬롯 설정")]
    public List<Image> slotImages;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        var inv = Inventory.Instance;
        if (inv == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) inv.ChangeSelectedSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inv.ChangeSelectedSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inv.ChangeSelectedSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inv.ChangeSelectedSlot(3);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
            inv.ChangeSelectedSlot((inv.GetCurrentIndex() + 1) % inv.GetInventorySize());
        else if (scroll < 0f)
            inv.ChangeSelectedSlot((inv.GetCurrentIndex() - 1 + inv.GetInventorySize()) % inv.GetInventorySize());

        UpdateInventoryUI();
        UpdateSlotHighlight(inv.GetCurrentIndex());
    }

    public void UpdateInventoryUI()
    {
        var inv   = Inventory.Instance;
        var items = inv.GetInventoryItems();
        int cnt   = Mathf.Min(slotImages.Count, items.Count);

        for (int i = 0; i < cnt; i++)
        {
            if (items[i] != null)
            {
                slotImages[i].sprite = items[i].itemImage;
                slotImages[i].color  = Color.white;
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].color  = new Color(1f,1f,1f,0f);
            }
        }

        for (int i = cnt; i < slotImages.Count; i++)
        {
            slotImages[i].sprite = null;
            slotImages[i].color  = new Color(1f,1f,1f,0f);
        }
    }

    public void UpdateSlotHighlight(int currentIndex)
    {
        for (int i = 0; i < slotImages.Count; i++)
        {
            var bg = slotImages[i].transform.parent?.GetComponent<Image>();
            if (bg != null)
                bg.color = (i == currentIndex) ? Color.yellow : Color.white;
        }
    }
}
