using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void RestartGame()
    {
        // 씬 로드 완료 시 콜백 등록
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

            // 한 번만 실행되도록 제거
            SceneManager.sceneLoaded -= OnCompanySceneLoaded;
        }
    }
}
