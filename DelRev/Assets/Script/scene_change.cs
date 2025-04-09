using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string nextSceneName = "Scene_move";
    public Vector3 spawnPosition = new Vector3(3.5f, 1.3834f, 19.55f);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 위치 저장
            PlayerPrefs.SetFloat("RespawnX", spawnPosition.x);
            PlayerPrefs.SetFloat("RespawnY", spawnPosition.y);
            PlayerPrefs.SetFloat("RespawnZ", spawnPosition.z);

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
