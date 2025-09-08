using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;         // 저장/계속/나가기 버튼 모음(비활성화로 시작)

    [Header("Refs")]
    public PlayerController player;  // 에디터 할당 또는 자동 탐색
    public Inventory inventory;      // 에디터 할당 또는 자동 탐색

    [Header("Settings")]
    public string startSceneName = "GameStart";
    public int saveSlot = 1;         // 드롭다운이 없으니 1번 슬롯 고정

    bool isOpen = false;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (inventory == null) inventory = FindObjectOfType<Inventory>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        isOpen = !isOpen;

        if (panel != null) panel.SetActive(isOpen);

        if (isOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 버튼 연결: 저장하기
    public void OnClickSave()
    {
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (inventory == null) inventory = FindObjectOfType<Inventory>();
        if (player == null || inventory == null)
        {
            Debug.LogWarning("[PauseMenu] Save 실패: Player 또는 Inventory 참조가 없습니다.");
            return;
        }

        SaveLoadManager.SaveGame(player, inventory, saveSlot);
        PlayerPrefs.SetInt("last_slot", saveSlot);
        PlayerPrefs.Save();
        Debug.Log($"[PauseMenu] 저장 완료 (슬롯 {saveSlot})");
    }

    // 버튼 연결: 계속하기
    public void OnClickResume()
    {
        ToggleMenu();
    }

    // 버튼 연결: 나가기(타이틀로)
    public void OnClickExitToStart()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(startSceneName);
    }
}
