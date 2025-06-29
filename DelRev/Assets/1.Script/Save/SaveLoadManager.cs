using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    private static string savePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void SaveGame(PlayerController player, Inventory inventory)
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
        File.WriteAllText(savePath, json);
        Debug.Log($"게임 저장 완료: {savePath}");
    }

    public static void LoadGame(PlayerController player, Inventory inventory)
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("저장 파일이 없습니다.");
            return;
        }

        string json = File.ReadAllText(savePath);
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
                    GameObject newItem = Instantiate(prefab);
                    Item itemComp = newItem.GetComponent<Item>();
                    inventory.SetItemAt(i, itemComp);
                    newItem.SetActive(false); // 인벤토리에만 있고 월드에 안 보이게
                }
            }
        }

        Debug.Log($"게임 로드 완료: {savePath}");
    }
}
