using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
  public GameObject helpPanel;
  public GameObject operatePanel;

  public void StartGame()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    SceneManager.LoadScene("Company");
  }

  public void ShowHelp()
  {
    helpPanel.SetActive(true);
    operatePanel.SetActive(false);
  }

  public void HideHelp()
  {
    helpPanel.SetActive(false);
  }

  public void ShowOperate()
  {
    helpPanel.SetActive(false);
    operatePanel.SetActive(true);
  }

  public void HideOperate()
  {
    operatePanel.SetActive(false);
    helpPanel.SetActive(true);
  }

  void Update()
  {
    if (helpPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
    {
      HideHelp();
    }

    if (operatePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
    {
      HideOperate();
    }
  }
}
