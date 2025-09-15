using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;              // 저장/계속/나가기 버튼이 들어있는 패널(비활성화 시작 권장)

    [Header("Refs")]
    public PlayerController player;       // 에디터 할당 or 자동 탐색
    public Inventory inventory;           // 에디터 할당 or 자동 탐색

    [Header("Settings")]
    public string startSceneName = "GameStart";
    public int saveSlot = 1;              // 슬롯 고정

    bool isOpen = false;
    float cachedFixedDeltaTime;

    void Awake()
    {
        cachedFixedDeltaTime = Time.fixedDeltaTime;

        // 1) Player/Inventory 자동 연결(없을 경우)
        if (player == null)    player    = FindObjectOfType<PlayerController>();
        if (inventory == null) inventory = FindObjectOfType<Inventory>();

        // 2) EventSystem 보장
        EnsureEventSystem();

        // 3) Pause 패널이 클릭을 받을 수 있도록 Canvas/GraphicRaycaster/CanvasGroup 보장
        EnsureUIClickability();

        // 4) 패널 비활성화 시작
        if (panel != null) panel.SetActive(false);
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
            // 일시정지
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0f; // 물리 업데이트 완전 정지(선택)
            // 커서
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;

            // 혹시 다른 전체화면 UI가 Raycast를 가로채면, 패널을 최상단으로
            panel.transform.SetAsLastSibling();
        }
        else
        {
            // 재개
            Time.timeScale = 1f;
            Time.fixedDeltaTime = cachedFixedDeltaTime;
            // 커서
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    // === 버튼 핸들러 ===

    public void OnClickSave()
    {
        if (player == null)    player    = FindObjectOfType<PlayerController>();
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

    public void OnClickResume()
    {
        if (!isOpen) return;
        ToggleMenu();
    }

    public void OnClickExitToStart()
    {
        // 시간/커서 원복
        Time.timeScale = 1f;
        Time.fixedDeltaTime = cachedFixedDeltaTime;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // 시작씬 가기 전에 DDOL 전부 제거
        GlobalState.KillAllDontDestroyOnLoad();

        SceneManager.LoadScene(startSceneName, LoadSceneMode.Single);
    }

    // === 보조: UI 클릭 보장 ===

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(go);
        }
    }

    void EnsureUIClickability()
    {
        if (panel == null) return;

        // 패널의 최상위 Canvas를 찾음(없으면 추가)
        var root = panel.transform as Transform;
        Canvas canvas = panel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = panel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // 최상단 정렬(다른 UI 위로)
        canvas.overrideSorting = true;
        if (canvas.sortingOrder < 1000) canvas.sortingOrder = 1000;

        // GraphicRaycaster 보장(클릭 레이캐스트)
        if (canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();

        // CanvasGroup으로 상호작용 허용
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }
}
