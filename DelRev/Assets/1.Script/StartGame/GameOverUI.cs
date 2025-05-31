using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
  public void RestartGame()
  {
    Vector3 spawnPos = new Vector3(-2.5f, 2f, 45f);
    transform.position = spawnPos;

    SceneManager.LoadScene("Company");
  }
}