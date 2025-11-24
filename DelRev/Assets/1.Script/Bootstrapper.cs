using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-3000)] // MapTracker(-1000)보다 더 먼저
public class Bootstrapper : MonoBehaviour
{
    // 에디터에 굳이 배치 안 해도 되게 런타임에 자동으로 뜨도록
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureSingletons()
    {
        // MapTracker가 없다면 만들어서 유지
        if (MapTracker.Instance == null)
        {
            var go = new GameObject("___Singletons");
            // 올바른 API 이름: DontDestroyOnLoad
            Object.DontDestroyOnLoad(go);
            go.AddComponent<MapTracker>();
            Debug.Log("[Bootstrapper] MapTracker 생성 및 보존");
        }

        // 첫 Company 진입 시 Day가 0으로 남는 경우를 위한 “응급 패치”
        SceneManager.sceneLoaded -= OnSceneLoadedOnceKick;
        SceneManager.sceneLoaded += OnSceneLoadedOnceKick;
    }

    
    // 새 게임용 재초기화 엔트리
    public static void EnsureAfterNewGame()
    {
        Debug.Log("[Bootstrapper] 새 게임 - 싱글톤 재초기화 시도");

        // KillAllDontDestroyOnLoad로 실제 오브젝트는 파괴 대기중일 수 있지만,
        // static Instance는 그대로일 수 있으니 직접 null로 초기화.
        MapTracker.Instance = null;

        EnsureSingletons();
    }

    private static bool _kickedOnce = false;
    private static void OnSceneLoadedOnceKick(Scene s, LoadSceneMode m)
    {
        if (_kickedOnce) return;
        if (s.name == "Company" && MapTracker.Instance != null)
        {
            // 시작 씬이 GameStart여도, Company 진입 시 Day가 최소 1이 되도록 보정
            if (MapTracker.Instance.currentDay <= 0)
            {
                MapTracker.Instance.currentDay = 1;
                Debug.Log("[Bootstrapper] 첫 Company 진입 감지 → Day를 1로 보정");
            }
            _kickedOnce = true;
        }
    }
}
