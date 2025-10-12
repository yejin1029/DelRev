using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 어디서든 SceneLoader.Load("Target")를 호출하면
/// 1) 로딩씬으로 이동 → 2) 로딩씬에서 Target을 비동기로 로드
/// </summary>
public static class SceneLoader
{
    // 로딩씬 이름(빌드 세팅에 등록되어 있어야 함)
    public const string LOADING_SCENE_NAME = "LoadingScene";

    // 다음에 로드할 실제 씬 이름을 임시 저장
    public static string NextSceneName { get; private set; }

    // 외부에서 호출
    public static void Load(string targetScene)
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("[SceneLoader] targetScene is null or empty");
            return;
        }

        // 타임스케일 0이라도 로딩 진행되도록 1로 복구(원하면 주석)
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        NextSceneName = targetScene;
        SceneManager.LoadScene(LOADING_SCENE_NAME, LoadSceneMode.Single);
    }
}
