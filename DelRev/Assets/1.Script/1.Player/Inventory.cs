using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Inventory : MonoBehaviour
{
    private List<Item> inventory = new List<Item>();
    private List<GameObject> inventoryObjects = new List<GameObject>();
    private int inventorySize = 4;
    private int currentIndex = 0;

    public bool isInputLocked = false;

    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip dropSound;
    [Range(0f, 1f)] public float pickupVolume = 0.2f;
    [Range(0f, 1f)] public float dropVolume = 0.2f;

    private AudioSource audioSource;
    private InventoryUI inventoryUI;

    void Awake()
    {
        inventoryUI = FindObjectOfType<InventoryUI>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 기본 볼륨 설정
        audioSource.volume = 1.0f; // PlayOneShot에서는 이건 무시됨
    }

    void Start()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            inventory.Add(null);
            inventoryObjects.Add(null);
        }
    }

    void Update()
    {
        if (isInputLocked) return;

        if (Input.GetKeyDown(KeyCode.E)) TryPickupItem();
        if (Input.GetKeyDown(KeyCode.G)) DropItem();

        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSelectedSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSelectedSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeSelectedSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeSelectedSlot(3);
    }

    void TryPickupItem()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 4f))
        {
            GameObject hitObject = hit.collider.gameObject;
            Item item = hitObject.GetComponent<Item>();

            if (item != null)
            {
                if (inventory[currentIndex] == null)
                {
                    inventory[currentIndex] = item;
                    inventoryObjects[currentIndex] = hitObject;
                    hitObject.SetActive(false);

                    if (pickupSound != null && audioSource != null)
                        audioSource.PlayOneShot(pickupSound, pickupVolume);
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

            Vector3 dropPosition = transform.position + transform.forward * 1f;
            droppedObject.transform.position = dropPosition;
            droppedObject.SetActive(true);

            if (dropSound != null && audioSource != null)
                audioSource.PlayOneShot(dropSound, dropVolume);
        }
    }

    public void ChangeSelectedSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
            return;

        currentIndex = slotIndex;
        inventoryUI?.UpdateSlotHighlight(currentIndex);
    }

    public void ClearInventory()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            inventory[i] = null;
            inventoryObjects[i] = null;
        }

        inventoryUI?.UpdateInventoryUI();
    }

    public void SetItemAt(int index, Item item)
    {
        if (index >= 0 && index < inventorySize)
        {
            inventory[index] = item;
            inventoryObjects[index] = null;
            inventoryUI?.UpdateInventoryUI();
        }
    }

    public List<Item> GetInventoryItems() => inventory;
    public int GetCurrentIndex() => currentIndex;
    public int GetInventorySize() => inventorySize;
}
