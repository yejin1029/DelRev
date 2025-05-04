using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationPanelManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject mapPanel;

    public NavigationButtonSelector mainSelector;
    public MapLocationSelector mapSelector;

    void Start()
    {
        mainSelector.DeactivateSelection();
        ShowMainPanel(false);
    }

    public void ShowMainPanel(bool activateSelector = true)
    {
        mainPanel.SetActive(true);
        mapPanel.SetActive(false);

        if (activateSelector)
        {
            mainSelector.ActivateSelection();
        }
        else
        {
            mainSelector.DeactivateSelection();
        }

    mapSelector.DeactivateSelection(); // 입력 비활성화
    }

    public void ShowMapPanel()
    {
        mainPanel.SetActive(false);
        mapPanel.SetActive(true);

        mapSelector.ActivateSelection();
        mainSelector.DeactivateSelection();
    }

    public bool IsMainPanelActive()
    {
        return mainPanel.activeSelf;
    }
}
