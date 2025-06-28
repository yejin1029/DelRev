using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void RestartGame()
    {
        // 1. MapTracker 값 전부 초기화
        if (MapTracker.Instance != null)
        {
            MapTracker.Instance.map1Count = 0;
            MapTracker.Instance.otherMapCount = 0;
            MapTracker.Instance.currentDay = 0;
            MapTracker.Instance.totalCoinCount = 0;
            MapTracker.Instance.isRestartingFromGameOver = true; // 복귀 플래그도 설정
        }

        // 2. Company 씬 로드 & 플레이어 위치 재배치
        SceneManager.sceneLoaded += OnCompanySceneLoaded;
        SceneManager.LoadScene("Company");
    }

    private void OnCompanySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Company")
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                player.transform.position = new Vector3(-4.5f, 2f, 45f);
            }

            SceneManager.sceneLoaded -= OnCompanySceneLoaded;
        }
    }
}
