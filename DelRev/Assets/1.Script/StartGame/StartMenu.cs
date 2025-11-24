using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class StartMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject helpPanel;
    public GameObject operatePanel;

    [Header("Settings")]
    public string firstGameSceneName = "Company"; // 새 게임 시작 씬
    public int defaultSlot = 1; // 기본 슬롯(단일 슬롯 운용)

    // ==== 버튼용 공개 메서드 ====

    // 새 게임(저장 무시하고 무조건 처음부터)
    public void StartNewGame()
    {
        StartNewGameInSlot(defaultSlot);
    }

    public void StartNewGameInSlot(int slot)
    {
        string path = SaveLoadManager.GetSavePath(slot);
        if (File.Exists(path))
            File.Delete(path); // 저장 파기

        // 새 게임 플래그
        PlayerPrefs.SetInt("last_slot", slot);
        PlayerPrefs.SetInt("__NEW_GAME__", 1);
        PlayerPrefs.Save();

        // 싱글톤 재초기화 (새 ___Singletons + MapTracker 생성)
        Bootstrapper.EnsureAfterNewGame();

        // 🔹 새 게임 시작 전에 DayManager 리셋
        if (DayManager.Instance != null)
        {
            DayManager.Instance.ResetForNewGame();
        }

        // 첫 게임 씬으로 이동
        SceneLoader.Load(firstGameSceneName);

        Debug.Log($"[StartMenu] 새 게임 시작 - 슬롯 {slot} (저장 삭제 & DDOL 정리)");
    }

    // 이어하기(저장 없으면 동작하지 않음)
    public void Continue()
    {
        int slot = PlayerPrefs.GetInt("last_slot", defaultSlot);
        if (!SaveLoadManager.HasSave(slot))
        {
            Debug.LogWarning("이어하기 불가: 저장 파일이 없습니다.");
            return;
        }
        ContinueLast();
    }

    // 가장 최근 슬롯으로 이어하기(씬 이동 + 데이터 주입)
    public void ContinueLast()
    {
        int slot = PlayerPrefs.GetInt("last_slot", defaultSlot);
        string path = SaveLoadManager.GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning("ContinueLast 실패: 저장 파일이 없습니다.");
            return;
        }

        // 새 게임 플래그 해제(안전)
        PlayerPrefs.SetInt("__NEW_GAME__", 0);
        PlayerPrefs.Save();

        // 실제 이어하기 시퀀스 실행
        // (참고) ContinueLoader 내부에서 씬 전환이 있다면
        // SceneManager.LoadScene(...) 대신 SceneLoader.Load(...)를 쓰도록 한 줄만 바꾸면,
        // 이어하기도 동일한 로딩 UX를 사용할 수 있어.
        ContinueLoader.Begin(slot);
    }

    // --- 도움말 열기 ---
    public void OpenHelpPanel()
    {
        if (helpPanel == null)
        {
            Debug.LogWarning("HelpPanel이 연결되지 않았습니다.");
            return;
        }

        // 패널 표시/숨김
        helpPanel.SetActive(true);
        if (operatePanel != null) operatePanel.SetActive(false);

        // EventSystem이 없으면 생성 (키보드/패드 네비게이션 대비)
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    // --- 도움말 닫기(닫기 버튼에 연결) ---
    public void OpenOperatePanel()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
        if (operatePanel != null) operatePanel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
        if (operatePanel != null) operatePanel.SetActive(false);
    }

    // (선택) ESC로 닫기 원하시면 Update에 추가
    void Update()
    {
        if (helpPanel != null && helpPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }
}
