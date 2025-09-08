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
        // --- 인벤토리 4칸만 저장 ---
        var names = new List<string>(4);
        var items = inventory.GetInventoryItems(); // 기존 프로젝트 메서드 사용
        for (int i = 0; i < 4; i++)
        {
            var item = (items != null && i < items.Count) ? items[i] : null;
            names.Add(item != null ? item.itemName : null);
        }

        SaveData data = new SaveData
        {
            health = player.health,
            stamina = player.stamina,
            coinCount = player.coinCount,
            playerPosition = player.transform.position,
            currentDay = MapTracker.Instance?.currentDay ?? 0,
            sceneName = SceneManager.GetActiveScene().name,
            inventoryItemNames = names
        };

        string json = JsonUtility.ToJson(data, true);

        // 원자적 저장
        string path = GetSavePath(slot);
        string tmp = path + ".tmp";
        File.WriteAllText(tmp, json);
        if (File.Exists(path)) File.Delete(path);
        File.Move(tmp, path);

        // 이어하기용 슬롯 기록
        PlayerPrefs.SetInt("last_slot", slot);
        PlayerPrefs.Save();

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

        if (MapTracker.Instance != null)
            MapTracker.Instance.currentDay = data.currentDay;

        // --- 인벤토리 4칸만 복원 ---
        var names = data.inventoryItemNames ?? new List<string>();
        for (int i = 0; i < 4; i++)
        {
            string itemName = (i < names.Count) ? names[i] : null;
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
                else
                {
                    // 리소스가 없으면 해당 칸 비워두기
                    inventory.SetItemAt(i, null);
                }
            }
            else
            {
                inventory.SetItemAt(i, null);
            }
        }

        PlayerPrefs.SetInt("last_slot", slot);
        PlayerPrefs.Save();

        Debug.Log($"게임 로드 완료 - 슬롯 {slot}: {path}");
    }
}
