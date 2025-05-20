using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneCleaner : MonoBehaviour
{
  void Awake()
  {
    var currentScene = SceneManager.GetActiveScene();

    var saveManager = FindObjectOfType<SaveLoadManager>();
    if (saveManager != null && saveManager.gameObject.scene.name == "DontDestroyOnLoad")
    {
      Destroy(saveManager.gameObject);
      Debug.Log("[StartSceneCleaner] SaveLoadManager 제거됨");
    }

    var player = GameObject.FindWithTag("Player");
    if (player != null && player.scene.name == "DontDestroyOnLoad")
    {
      Destroy(player);
      Debug.Log("[StartSceneCleaner] Player 제거됨");
    }

    foreach (var cam in FindObjectsOfType<Camera>())
    {
      var sceneName = cam.gameObject.scene.name;
      Debug.Log($"[StartSceneCleaner] 카메라 확인 중: {cam.name}, scene = {sceneName}");

      if (sceneName == "DontDestroyOnLoad")
      {
        Destroy(cam.gameObject);
        Debug.Log($"[StartSceneCleaner] 카메라 제거됨: {cam.name}");
      }
    }
  }
}
