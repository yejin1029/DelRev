using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private List<Item> inventory = new List<Item>();
    private List<GameObject> inventoryObjects = new List<GameObject>();
    private int inventorySize = 4;
    private int currentIndex = 0; // Tracks the currently selected inventory slot

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Find the closest object with an Item component
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f); // Adjust radius as needed
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
                if (inventory.Count < inventorySize)
                {
                    inventory.Add(item);
                    closestObject.SetActive(false); // Ensure the object is active in the scene
                    inventoryObjects.Add(closestObject); // Store the object reference in the inventoryObjects list
                    Debug.Log($"Picked up: {item.itemName}. Inventory: {string.Join(", ", inventory.ConvertAll(i => i?.itemName ?? "Empty"))}");
                }
                else
                {
                    Debug.Log("Inventory is full!");
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            DropItem();
        }

        // Change the currently selected inventory slot using keys 1, 2, 3, 4
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSelectedSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSelectedSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeSelectedSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeSelectedSlot(3);
    }

    void DropItem()
    {
        if (inventory.Count > 0 && currentIndex < inventory.Count && inventory[currentIndex] != null)
        {
            Item droppedItem = inventory[currentIndex];
            inventory[currentIndex] = null;
            GameObject droppedObject = inventoryObjects[currentIndex];
            inventoryObjects[currentIndex] = null;

            // Instantiate the dropped item in front of the player
            Vector3 dropPosition = transform.position + transform.forward * 2f; // Adjust the distance as needed
            droppedObject.transform.position = dropPosition;
            droppedObject.SetActive(true); // Ensure the object is active in the scene
            droppedObject = null;

            Debug.Log($"Dropped: {droppedItem.itemName}. Inventory: {string.Join(", ", inventory.ConvertAll(i => i?.itemName ?? "Empty"))}");
        }
        else
        {
            Debug.Log("No item to drop in the selected slot!");
        }
    }

    void ChangeSelectedSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
        {
            Debug.Log("Invalid slot index!");
            return;
        }

        currentIndex = slotIndex;
        if (currentIndex < inventory.Count && inventory[currentIndex] != null)
        {
            Debug.Log($"Selected slot {currentIndex + 1}. Current item: {inventory[currentIndex].itemName}");
        }
        else
        {
            Debug.Log($"Selected slot {currentIndex + 1}. No item in this slot.");
        }
    }
}