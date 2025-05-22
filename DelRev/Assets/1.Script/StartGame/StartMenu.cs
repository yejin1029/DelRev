using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
  public GameObject helpPanel;

  public void StartGame()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    SceneManager.LoadScene("PlayerTest");
  }

  public void ShowHelp()
  {
    helpPanel.SetActive(true);
  }

  public void HideHelp()
  {
    helpPanel.SetActive(false);
  }

  void Update()
  {
    if (helpPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
    {
      HideHelp();
    }
  }
}
