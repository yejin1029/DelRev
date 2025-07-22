using UnityEngine;

public class StorePanelController : MonoBehaviour
{
    public NavigationPanelManager panelManager;
    public StorePanelSelector selector; // 상점 키보드 선택기 연결

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            panelManager.ShowMainPanel(); // ESC로 메인 패널로 복귀
        }
    }

    void OnEnable()
    {
        if (selector != null)
            selector.enabled = true; // 상점 패널이 열릴 때 키보드 선택기 활성화
    }

    void OnDisable()
    {
        if (selector != null)
            selector.enabled = false; // 상점 패널 닫히면 비활성화
    }
}
