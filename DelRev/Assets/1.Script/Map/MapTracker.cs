using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (sceneName == "GameStart" || sceneName == "GameOver")
            return;

        if (isRestartingFromGameOver)
        {
            if (sceneName == "Company")
            {
                map1Count = 1;
                otherMapCount = 0;
                currentDay = 0;
                totalCoinCount = 0;
                Debug.Log("[MapTracker] GameOver 복귀 → Company 카운트 1로 설정");
            }

            isRestartingFromGameOver = false;
            return;
        }

        int prevMap1 = map1Count;
        int prevOther = otherMapCount;

        if (sceneName == "Company")
            map1Count++;
        else
            otherMapCount++;

        if (map1Count == otherMapCount && prevMap1 != prevOther)
        {
            currentDay++;
            Debug.Log($"📅 Day advanced! 현재 {currentDay}일차");

            // 💥 코인 요구 검증
            int index = checkDays.IndexOf(currentDay);
            if (index != -1 && index < coinRequirements.Count)
            {
                int required = coinRequirements[index];
                if (totalCoinCount < required)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        var controller = player.GetComponent<PlayerController>();
                        if (controller != null)
                        {
                            controller.TakeDamage(200f);
                            Debug.LogWarning($"[MapTracker] {currentDay}일차에 {required}코인 미달 → -200 데미지!");
                        }
                    }
                }
            }
        }

        Debug.Log($"Company: {map1Count} / Other: {otherMapCount} / Day: {currentDay}");
    }
}
