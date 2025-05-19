using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeToStart : MonoBehaviour
{
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      StartCoroutine(SaveAndReturnToStart());
    }
  }

  private IEnumerator SaveAndReturnToStart()
  {
    var saveManager = FindObjectOfType<SaveLoadManager>();
    if (saveManager != null)
    {
      saveManager.SaveGame();
    }
    yield return null;
    SceneManager.LoadScene("StartScene");
  }
}
