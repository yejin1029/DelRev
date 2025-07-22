using System.Collections;
using UnityEngine;
using TMPro;

public class WarningUI : MonoBehaviour
{
    public static WarningUI Instance { get; private set; }

    public TextMeshProUGUI warningText;
    public GameObject warningPanel;
    public float displayTime = 2f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        warningPanel.SetActive(false);
    }

    public void ShowWarning(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowWarningCoroutine(message));
    }

    private IEnumerator ShowWarningCoroutine(string message)
    {
        warningText.text = message;
        warningPanel.SetActive(true);
        yield return new WaitForSeconds(displayTime);
        warningPanel.SetActive(false);
    }
}
