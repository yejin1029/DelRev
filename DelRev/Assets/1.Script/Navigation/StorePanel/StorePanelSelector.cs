using UnityEngine;

public class StorePanelSelector : MonoBehaviour
{
    public StoreItemUI[] itemUIs;
    private int selectedIndex = 0;
    private bool isActive = false;

    void OnEnable()
    {
        if (itemUIs == null || itemUIs.Length == 0) return;

        isActive = true;
        selectedIndex = 0; // 패널 열릴 때 항상 첫 번째 버튼 선택
        UpdateSelection();
    }

    void OnDisable()
    {
        isActive = false;
        ClearSelection();
    }

    void Update()
    {
        if (!isActive) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex = (selectedIndex - 1 + itemUIs.Length) % itemUIs.Length;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex = (selectedIndex + 1) % itemUIs.Length;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            itemUIs[selectedIndex].SendMessage("OnBuy");
        }
    }

    void UpdateSelection()
    {
        for (int i = 0; i < itemUIs.Length; i++)
        {
            itemUIs[i].SetSelected(i == selectedIndex);
        }
    }

    void ClearSelection()
    {
        foreach (var ui in itemUIs)
        {
            ui.SetSelected(false);
        }
    }
}
