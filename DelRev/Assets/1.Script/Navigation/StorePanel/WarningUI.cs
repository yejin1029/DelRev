using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WarningUI : MonoBehaviour
{
    public static WarningUI Instance { get; private set; }

    public GameObject warningPanel;
    public GameObject storePanel;
    public GameObject mainPanel;
    public TextMeshProUGUI warningText;
    public Button closeButton;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (warningPanel != null)
            warningPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseWarning);
    }

    void Update()
    {
        if (!warningPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            closeButton.onClick.Invoke(); // 버튼 강제 클릭
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            warningPanel.SetActive(false); // 경고창 닫기
            if (mainPanel != null) mainPanel.SetActive(false); // 이전 패널 닫기
            if (storePanel != null) storePanel.SetActive(true); // 상점 패널 켜기
        }
    }

    public void ShowWarning(string message)
    {
        if (warningText != null)
            warningText.text = message;

        if (warningPanel != null)
            warningPanel.SetActive(true);

        // UI 조작 잠금 등 추가 가능
    }

    public void CloseWarning()
    {
        if (warningPanel != null)
            warningPanel.SetActive(false);

        // Store 패널 자동 복귀 (원하면 NavigationPanelManager 통해)
        var storePanel = FindObjectOfType<StorePanelController>();
        if (storePanel != null)
            storePanel.gameObject.SetActive(true);
    }
}
