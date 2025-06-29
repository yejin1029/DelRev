using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class StartMenu : MonoBehaviour
{
  public GameObject helpPanel;
  public GameObject operatePanel;

  public void StartGame()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    string path = Path.Combine(Application.persistentDataPath, "save.json");
    if (File.Exists(path))
        File.Delete(path); // 기존 저장 삭제

    SceneManager.LoadScene("Company");
  }

  public void ShowHelp()
  {
    helpPanel.SetActive(true);
    operatePanel.SetActive(false);
  }

  public void LoadGameFromMenu()
  {
    string path = Path.Combine(Application.persistentDataPath, "save.json");
    if (!File.Exists(path))
    {
      Debug.LogWarning("저장된 파일이 없습니다!");
      return;
    }

    // 저장된 데이터에서 씬 이름 미리 추출
    string json = File.ReadAllText(path);
    SaveData data = JsonUtility.FromJson<SaveData>(json);

    if (string.IsNullOrEmpty(data.sceneName))
    {
        Debug.LogWarning("⚠ 저장된 씬 이름이 없습니다. 기본 'Company'로 이동");
        StartCoroutine(LoadAndMoveScene("Company"));
    }
    else
    {
        StartCoroutine(LoadAndMoveScene(data.sceneName));
    }
  }

IEnumerator LoadAndMoveScene(string sceneName)
{
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

    while (!asyncLoad.isDone)
        yield return null;

    PlayerController player = null;
    Inventory inventory = null;

    float timeout = 3f;
    while ((player == null || inventory == null) && timeout > 0f)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        inventory = FindObjectOfType<Inventory>();
        timeout -= Time.deltaTime;
        yield return null;
    }

    if (player != null && inventory != null)
    {
        SaveLoadManager.LoadGame(player, inventory);
        Debug.Log($"📂 저장된 게임 불러오기 완료: {sceneName}");
    }
    else
    {
        Debug.LogWarning("❌ Player 또는 Inventory를 찾지 못했습니다.");
    }
}

  public void HideHelp()
  {
    helpPanel.SetActive(false);
  }

  public void ShowOperate()
  {
    helpPanel.SetActive(false);
    operatePanel.SetActive(true);
  }

  public void HideOperate()
  {
    operatePanel.SetActive(false);
    helpPanel.SetActive(true);
  }

  void Update()
  {
    if (helpPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
    {
      HideHelp();
    }

    if (operatePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
    {
      HideOperate();
    }
  }
}
