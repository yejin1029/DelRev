using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
  void Start()
  {
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }

  public void OnNewGame()
  {
    // 기존 저장 데이터 완전히 초기화
    SaveLoadManager resetter = FindObjectOfType<SaveLoadManager>();
    resetter?.ResetSave();

    PlayerPrefs.SetInt("SavedGame", 1);
    PlayerPrefs.Save();

    SceneManager.LoadScene("PlayerTest");
  }

  public void OnContinue()
  {
    if (PlayerPrefs.HasKey("SavedGame"))
    {
      SceneManager.LoadScene("PlayerTest");
    }
    else
    {
      Debug.Log("저장된 게임이 없습니다!");
    }
  }

  public void OnSettings()
  {
    // 설정 메뉴 열기
    Debug.Log("Settings clicked");
  }
}