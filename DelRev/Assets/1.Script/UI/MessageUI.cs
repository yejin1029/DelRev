using UnityEngine;
using TMPro;
using System.Collections;

public class MessageUI : MonoBehaviour
{
  public TextMeshProUGUI messageText;

  private Coroutine currentMessageCoroutine;

  public void ShowMessage(string message, float duration = 2f)
  {
    Debug.Log($"[MessageUI] ShowMessage: {message}");

    if (currentMessageCoroutine != null)
      StopCoroutine(currentMessageCoroutine);

    currentMessageCoroutine = StartCoroutine(DisplayMessage(message, duration));
  }

  private IEnumerator DisplayMessage(string message, float duration)
  {
    messageText.text = message;
    messageText.gameObject.SetActive(true);

    yield return new WaitForSeconds(duration);

    messageText.gameObject.SetActive(false);
    currentMessageCoroutine = null;
  }
}
