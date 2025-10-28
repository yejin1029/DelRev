using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[DefaultExecutionOrder(-1000)] // MapTracker를 가장 먼저 초기화
public class MapTracker : MonoBehaviour
{
    public static MapTracker Instance;

    public int map1Count = 0;
    public int otherMapCount = 0;
    public int totalCoinCount = 0;
    public int currentDay = 0;

    public bool isRestartingFromGameOver = false;

    // 🔹 외부에서 설정할 요일과 코인 요구량
    public List<int> checkDays = new List<int> { 4, 7, 9, 11, 13 };
    public List<int> coinRequirements = new List<int> { 5, 10, 15, 20, 25 };

    // Company에 들어왔을 때 알림: (isReturning, day)
    public static event Action<bool, int> CompanyEntered;
    
    // "Company 떠났다가 다시 들어왔는지" 추적
    private bool leftCompanySinceLastVisit = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(int amount)
    {
        totalCoinCount += amount;
        Debug.Log($"[MapTracker] 코인 +{amount}, 총 보유 코인: {totalCoinCount}");
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LoadingScene")
            return;

        // SceneLoader를 거치든 아니든 안전하게 이름 결정
        string sceneName = string.IsNullOrEmpty(SceneLoader.NextSceneName) ? scene.name : SceneLoader.NextSceneName;
        bool isCompany = sceneName.Contains("Company");
        Debug.Log($"[MapTracker] 씬 로드됨: {sceneName}");

        if (sceneName == "GameStart" || sceneName == "GameOver")
            return;

        if (isRestartingFromGameOver)
        {
            if (isCompany)
            {
                map1Count = 1;
                otherMapCount = 0;
                currentDay = 0;
                totalCoinCount = 0;
                leftCompanySinceLastVisit = false;
            }
            isRestartingFromGameOver = false;
            return;
        }

        // 첫 Company 진입 → Day 1
        if (isCompany && currentDay == 0)
        {
            currentDay = 1;
            map1Count++;
            leftCompanySinceLastVisit = false;
            CompanyEntered?.Invoke(false, currentDay);
            Debug.Log("[MapTracker] 첫 Company 진입 → Day 1 & 이벤트 발송");
            return;
        }

        // 돌아왔는지/떠났는지에 따라 날짜 및 플래그 처리
        if (isCompany)
        {
            map1Count++;

            bool isReturning = leftCompanySinceLastVisit; // 직전에 ‘다른 맵’을 다녀왔는가
            if (isReturning)
            {
                currentDay++;
                leftCompanySinceLastVisit = false;
                Debug.Log($"📅 Company 복귀 → Day {currentDay}");
            }

            // Company 진입 사실을 확실히 알림(메시지/튜토리얼/UI는 여기 구독)
            CompanyEntered?.Invoke(isReturning, currentDay);
        }
        else
        {
            otherMapCount++;
            leftCompanySinceLastVisit = true; // Company를 떠남
        }
    }
}
