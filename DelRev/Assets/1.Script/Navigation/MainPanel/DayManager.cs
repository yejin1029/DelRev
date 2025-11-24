using UnityEngine;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    public int CurrentDay { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Company 씬 들어갈 때 감지하기 위해 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log($"[DayManager] Awake, id = {GetInstanceID()}");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
            Debug.Log($"[DayManager] OnDestroy, id = {GetInstanceID()}");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 여기서는 이름만 보고 Company 입장 판단
        if (scene.name.Contains("Company"))
        {
            OnEnterCompany();
        }
    }

    public void ResetForNewGame()
    {
        CurrentDay = 0;
        Debug.Log("[DayManager] 새 게임 → Day 0으로 리셋");
    }

    private void OnEnterCompany()
    {
        if (CurrentDay <= 0)
            CurrentDay = 1;       // 첫 Company 입장
        else
            CurrentDay++;         // 그 이후 Company 재입장 = Day++

        Debug.Log($"[DayManager] Company 입장 → Day {CurrentDay}");
    }
}
