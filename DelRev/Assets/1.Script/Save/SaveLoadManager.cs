using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    public static string GetSavePath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    public static void SaveGame(PlayerController player, Inventory inventory, int slot)
    {
        SaveData data = new SaveData
        {
            health = player.health,
            stamina = player.stamina,
            coinCount = player.coinCount,
            playerPosition = player.transform.position,
            currentDay = MapTracker.Instance?.currentDay ?? 0,
            sceneName = SceneManager.GetActiveScene().name,
            inventoryItemNames = new List<string>()
        };

        foreach (var item in inventory.GetInventoryItems())
        {
            data.inventoryItemNames.Add(item != null ? item.itemName : null);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(slot), json);
        Debug.Log($"게임 저장 완료 - 슬롯 {slot}: {GetSavePath(slot)}");
    }

    public static void LoadGame(PlayerController player, Inventory inventory, int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"슬롯 {slot}에 저장된 파일이 없습니다.");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        player.health = data.health;
        player.stamina = data.stamina;
        player.coinCount = data.coinCount;
        player.transform.position = data.playerPosition;
        MapTracker.Instance.currentDay = data.currentDay;

        for (int i = 0; i < data.inventoryItemNames.Count; i++)
        {
            string itemName = data.inventoryItemNames[i];
            if (!string.IsNullOrEmpty(itemName))
            {
                GameObject prefab = Resources.Load<GameObject>($"Items/{itemName}");
                if (prefab != null)
                {
                    GameObject newItem = UnityEngine.Object.Instantiate(prefab);
                    Item itemComp = newItem.GetComponent<Item>();
                    inventory.SetItemAt(i, itemComp);
                    newItem.SetActive(false);
                }
            }
        }

        Debug.Log($"게임 로드 완료 - 슬롯 {slot}: {path}");
    }
}