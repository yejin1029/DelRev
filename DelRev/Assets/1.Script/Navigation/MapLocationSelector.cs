using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapLocationSelector : MonoBehaviour
{
  public Image[] locations;                  // 원 이미지들
  public Color normalColor = Color.white;
  public Color selectedColor = Color.yellow;
  public NavigationPanelManager panelManager;

  private int selectedIndex = 0;
  private bool isActive = false;

  public GameObject[] detailPanels;
  private GameObject currentDetailPanel;

  public void ActivateSelection()
  {
    isActive = true;
    selectedIndex = 0;
    UpdateVisuals();
  }

  public void DeactivateSelection()
  {
    isActive = false;
  }

  void OnEnable()
  {
    ActivateSelection();
  }

  void OnDisable()
  {
    DeactivateSelection();
  }

  void Update()
  {
    if (!isActive) return;

    // 맵 상세 패널이 열려있으면 방향 입력을 막음
    if (currentDetailPanel == null)
    {
      // 방향키 입력
      if (Input.GetKeyDown(KeyCode.W))
      {
        selectedIndex = GetNextByInput(Vector2.up);
        UpdateVisuals();
      }
      if (Input.GetKeyDown(KeyCode.S))
      {
        selectedIndex = GetNextByInput(Vector2.down);
        UpdateVisuals();
      }
      if (Input.GetKeyDown(KeyCode.A))
      {
        selectedIndex = GetNextByInput(Vector2.left);
        UpdateVisuals();
      }
      if (Input.GetKeyDown(KeyCode.D))
      {
        selectedIndex = GetNextByInput(Vector2.right);
        UpdateVisuals();
      }

      if (Input.GetKeyDown(KeyCode.Return))
      {
        Debug.Log($"선택된 위치: {selectedIndex}");

        if (currentDetailPanel != null)
          currentDetailPanel.SetActive(false);

        if (selectedIndex >= 0 && selectedIndex < detailPanels.Length)
        {
          currentDetailPanel = detailPanels[selectedIndex];
          currentDetailPanel.SetActive(true);

          currentDetailPanel.GetComponent<DetailPanelSelector>()?.Activate();
        }
      }
    }

    // 이전 화면 혹은 메인 화면으로 돌아가기
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      if (currentDetailPanel != null)
      {
        currentDetailPanel.SetActive(false);
        currentDetailPanel = null;
      }
      else
      {
        panelManager.ShowMainPanel();
      }
    }
  }

  void UpdateVisuals()
  {
    for (int i = 0; i < locations.Length; i++)
    {
      locations[i].color = (i == selectedIndex) ? selectedColor : normalColor;
    }
  }

  int GetNextByInput(Vector2 dir)
  {
    switch (selectedIndex)
    {
      case 0: // 1 (왼쪽위)
        if (dir == Vector2.right) return 2;
        if (dir == Vector2.down) return 1;
        break;
      case 1: // 2 (왼쪽아래)
        if (dir == Vector2.up) return 0;
        if (dir == Vector2.right) return 2;
        break;
      case 2: // 3 (가운데)
        if (dir == Vector2.left) return 0;
        if (dir == Vector2.right) return 3;
        break;
      case 3: // 4 (오른쪽위)
        if (dir == Vector2.left) return 2;
        if (dir == Vector2.down) return 4;
        break;
      case 4: // 5 (오른쪽아래)
        if (dir == Vector2.up) return 3;
        if (dir == Vector2.left) return 2;
        break;
    }
    return selectedIndex; // 이동 불가 시 현재 유지
  }

  public void ReactivateFromDetail()
  {
    currentDetailPanel = null;      // 현재 열린 패널 해제
    ActivateSelection();            // 입력 다시 켜기
  }
}