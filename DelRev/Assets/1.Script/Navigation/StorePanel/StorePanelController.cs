using UnityEngine;

public class StorePanelController : MonoBehaviour
{
    public NavigationPanelManager panelManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            panelManager.ShowMainPanel(); // ESC → MainPanel로 복귀
        }
    }

    void OnEnable()
    {
        // 필요 시 초기화 가능
    }
}
