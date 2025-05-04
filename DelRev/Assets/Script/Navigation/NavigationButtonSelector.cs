using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.UI;

public class NavigationButtonSelector : MonoBehaviour
{
  public Button[] buttons;
  public GameObject[] selectionOutlines; // 선택 중인 버튼 테두리 표시
  public NavigationPanelManager panelManager;

  public int selectedIndex = 0;
  private bool isActive = false;

  void Start()
  {
    HideOutlines();
  }

  void Update()
  {
    if (!isActive) return;

    // 좌우 선택 이동
    if (Input.GetKeyDown(KeyCode.A))
    {
      selectedIndex = Mathf.Max(0, selectedIndex - 1);
      UpdateButtonVisuals();
    }
    else if (Input.GetKeyDown(KeyCode.D))
    {
      selectedIndex = Mathf.Min(buttons.Length - 1, selectedIndex + 1);
      UpdateButtonVisuals();
    }

    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
    {
      if (selectedIndex == 0)
      {
        panelManager.ShowMapPanel(); // ← 왼쪽 버튼 클릭 시 맵 패널로 전환
      }
      else
      {
        Debug.Log("오른쪽 버튼 선택됨");
        // 다른 처리 있으면 여기에 추가
      }
    }
  }

  public void ActivateSelection()
  {
    isActive = true;
    selectedIndex = 0; // 왼쪽부터 시작
    UpdateButtonVisuals();
  }

  public void DeactivateSelection()
  {
    isActive = false;
    HideOutlines();
  }

  void UpdateButtonVisuals()
  {
    for (int i = 0; i < selectionOutlines.Length; i++)
    {
      selectionOutlines[i].SetActive(i == selectedIndex);
    }
  }

  void HideOutlines()
  {
    foreach (var outline in selectionOutlines)
    {
      outline.SetActive(false);
    }
  }
}

