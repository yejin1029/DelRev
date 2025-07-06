using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    private List<Item> inventory          = new List<Item>();
    private List<GameObject> inventoryObjects = new List<GameObject>();
    private int inventorySize             = 4;
    private int currentIndex              = 0;

    public bool isInputLocked             = false;

    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip dropSound;
    [Range(0f,1f)] public float pickupVolume = 0.2f;
    [Range(0f,1f)] public float dropVolume   = 0.2f;

    private AudioSource audioSource;

    void Awake()
    {
        // 싱글톤 & 씬 전환 불가
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.volume = 1f;

            for (int i = 0; i < inventorySize; i++)
            {
                inventory.Add(null);
                inventoryObjects.Add(null);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (isInputLocked) return;

        if (Input.GetKeyDown(KeyCode.E))    TryPickupItem();
        if (Input.GetKeyDown(KeyCode.G))    DropItem();

        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSelectedSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSelectedSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeSelectedSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeSelectedSlot(3);
    }

    void TryPickupItem()
    {
        if (Camera.main == null) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 4f))
        {
            GameObject go = hit.collider.gameObject;
            Item item = go.GetComponent<Item>();

            if (item != null && inventory[currentIndex] == null)
            {
                // 효과 적용 (IInventoryEffect 인터페이스 구현 시)
                if (item is IInventoryEffect effect)
                {
                    var player = FindObjectOfType<PlayerController>();
                    effect.OnAdd(player);
                }

                inventory[currentIndex]        = item;
                inventoryObjects[currentIndex] = go;

                // 씬 전환에도 보존
                go.transform.SetParent(null);
                DontDestroyOnLoad(go);
                go.SetActive(false);

                if (pickupSound != null)
                    audioSource.PlayOneShot(pickupSound, pickupVolume);

                InventoryUI.Instance?.UpdateInventoryUI();
                InventoryUI.Instance?.UpdateSlotHighlight(currentIndex);
            }
        }
    }

    void DropItem()
    {
        if (inventory[currentIndex] == null) return;

        // 효과 해제
        var itemToRemove = inventory[currentIndex];
        if (itemToRemove is IInventoryEffect effectRemove)
        {
            var player = FindObjectOfType<PlayerController>();
            effectRemove.OnRemove(player);
        }

        GameObject go = inventoryObjects[currentIndex];
        inventory[currentIndex]        = null;
        inventoryObjects[currentIndex] = null;

        go.transform.SetParent(null);
        go.transform.position = transform.position + transform.forward * 1f;
        go.SetActive(true);

        if (dropSound != null)
            audioSource.PlayOneShot(dropSound, dropVolume);

        InventoryUI.Instance?.UpdateInventoryUI();
        InventoryUI.Instance?.UpdateSlotHighlight(currentIndex);
    }

    public void ChangeSelectedSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize) return;
        currentIndex = slotIndex;
        InventoryUI.Instance?.UpdateSlotHighlight(currentIndex);
    }

    /// <summary>
    /// SaveLoadManager.LoadGame 에서 호출하여 불러온 아이템을 슬롯에 세팅합니다.
    /// 이때도 OnAdd 효과를 적용합니다.
    /// </summary>
    public void SetItemAt(int index, Item item)
    {
        if (index < 0 || index >= inventorySize || item == null) return;

        // 효과 적용
        if (item is IInventoryEffect effect)
        {
            var player = FindObjectOfType<PlayerController>();
            effect.OnAdd(player);
        }

        inventory[index]        = item;
        GameObject go           = item.gameObject;
        inventoryObjects[index] = go;

        go.transform.SetParent(null);
        DontDestroyOnLoad(go);
        go.SetActive(false);

        InventoryUI.Instance?.UpdateInventoryUI();
    }

    // 외부에서 현재 인벤토리 상태를 읽어갈 때 사용
    public List<Item> GetInventoryItems() => inventory;
    public int GetCurrentIndex()     => currentIndex;
    public int GetInventorySize()    => inventorySize;
}
