using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class SaveLoadManager : MonoBehaviour
{
  public PlayerController player;
  public Inventory inventory;
  public Transform cameraTransform;

  private string savePath;

  void Awake()
  {
    savePath = Application.persistentDataPath + "/save.json";
    Debug.Log("[SaveLoadManager] Awake");

    if (player == null)
    {
      player = FindObjectOfType<PlayerController>();
    }

    if (inventory == null)
    {
      inventory = FindObjectOfType<Inventory>();
    }

    if (cameraTransform == null && player != null)
    {
      cameraTransform = player.cameraTransform;
    }
  }

  void Start()
  {
    Debug.Log("[SaveLoadManager] Start");

    if (PlayerPrefs.HasKey("SavedGame"))
    {
      Debug.Log("[SaveLoadManager] 이어하기 감지됨 → LoadGame 호출");
      StartCoroutine(DelayedLoad());
    }
    else
    {
      var spawn = GameObject.Find("PlayerSpawnPoint");
      if (spawn != null && player != null)
      {
        player.controller.enabled = false;
        player.transform.position = spawn.transform.position;
        player.controller.enabled = true;
      }
    }
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.F5)) SaveGame();
    if (Input.GetKeyDown(KeyCode.F9)) LoadGame();
  }

  public void SaveGame()
  {
    PlayerPrefs.SetInt("SavedGame", 1);
    PlayerPrefs.Save();

    SaveData data = new SaveData();

    Vector3 pos = player.transform.position;
    data.playerPosition = new float[] { pos.x, pos.y, pos.z };

    data.health = player.health;
    data.stamina = player.stamina;
    data.coinCount = player.coinCount;

    var items = inventory.GetInventoryItems();
    data.inventoryItemNames = new List<string>();
    foreach (var item in items)
    {
      data.inventoryItemNames.Add(item != null ? item.itemName : "");
    }

    data.currentInventoryIndex = inventory.GetCurrentIndex();

    Vector3 localPos = cameraTransform.localPosition;
    Vector3 localRot = cameraTransform.localEulerAngles;
    data.cameraLocalPosition = new float[] { localPos.x, localPos.y, localPos.z };
    data.cameraLocalRotation = new float[] { localRot.x, localRot.y, localRot.z };

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

    Vector3 pos = new Vector3(data.playerPosition[0], data.playerPosition[1], data.playerPosition[2]);

    if (pos.y < 1f)
    {
      var spawn = GameObject.Find("PlayerSpawnPoint");
      if (spawn != null)
      {
        Debug.LogWarning("플레이어 위치가 너무 낮아 SpawnPoint로 보정");
        pos = spawn.transform.position;
      }
      else
      {
        pos.y = 3f;
      }
    }

    StartCoroutine(MovePlayerNextFrame(pos));

    player.health = data.health;
    player.stamina = data.stamina;
    player.coinCount = data.coinCount;

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

    if (cameraTransform != null && data.cameraLocalPosition != null && data.cameraLocalRotation != null)
    {
      Vector3 camLocalPos = new Vector3(
          data.cameraLocalPosition[0],
          data.cameraLocalPosition[1],
          data.cameraLocalPosition[2]
      );
      cameraTransform.localPosition = camLocalPos;

      Vector3 camLocalRot = new Vector3(
          data.cameraLocalRotation[0],
          data.cameraLocalRotation[1],
          data.cameraLocalRotation[2]
      );
      cameraTransform.localEulerAngles = camLocalRot;
    }

    Debug.Log("게임 불러오기 완료");
  }

  public void ResetSave()
  {
    if (File.Exists(savePath)) File.Delete(savePath);
    PlayerPrefs.DeleteKey("SavedGame");
    Debug.Log("저장 데이터 초기화 완료");
  }

  private IEnumerator MovePlayerNextFrame(Vector3 targetPosition)
  {
    yield return null;
    yield return null;

    player.controller.enabled = false;
    player.transform.position = targetPosition;
    yield return null;
    player.controller.enabled = true;

    Debug.Log("플레이어 위치 이동 완료: " + targetPosition);
  }

  private IEnumerator DelayedLoad()
  {
    yield return null;
    yield return null;
    LoadGame();
  }
}
