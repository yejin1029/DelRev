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
    // PlayerPrefs 키 삭제
    PlayerPrefs.DeleteKey("SavedGame");

    // 저장된 json 파일도 삭제
    string savePath = Application.persistentDataPath + "/save.json";
    if (System.IO.File.Exists(savePath))
    {
      System.IO.File.Delete(savePath);
      Debug.Log("[StartMenu] 이전 저장 파일 삭제됨");
    }

    // 새 게임 시작
    SceneManager.LoadScene("PlayerTest");
  }

  public void OnContinue()
  {
    if (PlayerPrefs.HasKey("SavedGame"))
    {
      Debug.Log("[StartMenu] 이어하기 선택됨 → PlayerTest 로드");
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