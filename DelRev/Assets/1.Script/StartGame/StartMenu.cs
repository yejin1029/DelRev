using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class StartMenu : MonoBehaviour
{
    public GameObject helpPanel;
    public GameObject operatePanel;

    private const int MaxSlots = 10;

    public void StartNewGameInSlot(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path); // 기존 슬롯 삭제
            Debug.Log($"슬롯 {slot}의 기존 저장 삭제됨");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene("Company");
    }

    public void LoadGameFromSlot(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"슬롯 {slot}에 저장된 파일이 없습니다!");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        string sceneToLoad = string.IsNullOrEmpty(data.sceneName) ? "Company" : data.sceneName;
        StartCoroutine(LoadAndMoveScene(sceneToLoad, slot));
    }

    private IEnumerator LoadAndMoveScene(string sceneName, int slot)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) yield return null;

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
            SaveLoadManager.LoadGame(player, inventory, slot);
            Debug.Log($"📂 슬롯 {slot}에서 게임 불러오기 완료: {sceneName}");
        }
        else
        {
            Debug.LogWarning("❌ Player 또는 Inventory를 찾지 못했습니다.");
        }
    }

    private string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }

    public void ShowHelp()
    {
        helpPanel.SetActive(true);
        operatePanel.SetActive(false);
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
