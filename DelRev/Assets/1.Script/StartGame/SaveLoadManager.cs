using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
  public PlayerController player;
  public Inventory inventory;

  private string savePath;

  void Start()
  {
    // 이어하기 상태라면 자동으로 불러오기
    if (PlayerPrefs.HasKey("SavedGame"))
    {
      LoadGame();
    }
  }

  void Awake()
  {
    savePath = Application.persistentDataPath + "/save.json";

    // 연결이 안 되어 있으면 자동으로 찾아 연결
    if (player == null)
      player = FindObjectOfType<PlayerController>();

    if (inventory == null)
      inventory = FindObjectOfType<Inventory>();
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.F5)) SaveGame();
    if (Input.GetKeyDown(KeyCode.F9)) LoadGame();
  }

  public void SaveGame()
  {
    SaveData data = new SaveData();

    // 플레이어 위치
    Vector3 pos = player.transform.position;
    data.playerPosition = new float[] { pos.x, pos.y, pos.z };

    // 상태 정보
    data.health = player.health;
    data.stamina = player.stamina;
    data.coinCount = player.coinCount;

    // 인벤토리 정보
    var items = inventory.GetInventoryItems();
    data.inventoryItemNames = new List<string>();
    foreach (var item in items)
    {
      data.inventoryItemNames.Add(item != null ? item.itemName : "");
    }

    data.currentInventoryIndex = inventory.GetCurrentIndex();

    // JSON 저장
    string json = JsonUtility.ToJson(data, true);
    File.WriteAllText(savePath, json);
    Debug.Log($"게임 저장됨: {savePath}");
  }

  public void LoadGame()
  {
    if (player == null || inventory == null)
    {
      Debug.LogError("SaveLoadManager: player 또는 inventory가 연결되지 않았습니다.");
      return;
    }

    if (!File.Exists(savePath))
    {
      Debug.LogWarning("저장 파일이 없습니다.");
      return;
    }

    string json = File.ReadAllText(savePath);
    SaveData data = JsonUtility.FromJson<SaveData>(json);

    // 위치 이동
    player.controller.enabled = false; // 위치 이동 전 충돌 방지
    Vector3 pos = new Vector3(data.playerPosition[0], data.playerPosition[1], data.playerPosition[2]);
    player.transform.position = pos;
    player.controller.enabled = true;

    // 상태 정보 복원
    player.health = data.health;
    player.stamina = data.stamina;
    player.coinCount = data.coinCount;

    // 인벤토리 복원
    inventory.ClearInventory();
    for (int i = 0; i < data.inventoryItemNames.Count; i++)
    {
      string itemName = data.inventoryItemNames[i];
      if (!string.IsNullOrEmpty(itemName))
      {
        Item prefab = ItemDatabase.GetItemByName(itemName);
        if (prefab != null)
        {
          inventory.SetItemAt(i, prefab);
        }
      }
    }

    inventory.ChangeSelectedSlot(data.currentInventoryIndex);
    Debug.Log("게임 불러오기 완료");
  }

  public void ResetSave()
  {
    string path = Application.persistentDataPath + "/save.json";
    if (File.Exists(path)) File.Delete(path);
    PlayerPrefs.DeleteKey("SavedGame");
  }
}
