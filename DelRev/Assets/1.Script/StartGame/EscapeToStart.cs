using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeToStart : MonoBehaviour
{
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      FindObjectOfType<SaveLoadManager>()?.SaveGame(); // 자동 저장
      SceneManager.LoadScene("StartScene");
    }
  }
}