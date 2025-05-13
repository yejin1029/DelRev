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
      if (Input.GetKeyDown(KeyCode.W)) Move(Vector2.up);
      if (Input.GetKeyDown(KeyCode.S)) Move(Vector2.down);
      if (Input.GetKeyDown(KeyCode.A)) Move(Vector2.left);
      if (Input.GetKeyDown(KeyCode.D)) Move(Vector2.right);

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

  void Move(Vector2 direction)
  {
    int nextIndex = GetClosestInDirection(direction);
    if (nextIndex != selectedIndex)
    {
      selectedIndex = nextIndex;
      UpdateVisuals();
    }
  }

  void UpdateVisuals()
  {
    for (int i = 0; i < locations.Length; i++)
    {
      locations[i].color = (i == selectedIndex) ? selectedColor : normalColor;
    }
  }

  int GetClosestInDirection(Vector2 dir)
{
    Vector2 currentPos = RectTransformUtility.WorldToScreenPoint(null, locations[selectedIndex].rectTransform.position);

    float closestDist = Mathf.Infinity;
    int bestIndex = selectedIndex;

    for (int i = 0; i < locations.Length; i++)
    {
        if (i == selectedIndex) continue;

        Vector2 targetPos = RectTransformUtility.WorldToScreenPoint(null, locations[i].rectTransform.position);
        Vector2 toTarget = (targetPos - currentPos).normalized;

        float dot = Vector2.Dot(toTarget, dir);
        float absAngle = Vector2.Angle(toTarget, dir);

        // 방향에 따라 필터링 (90도 이내면 허용)
        if (absAngle < 90f)
        {
            float dist = Vector2.Distance(currentPos, targetPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                bestIndex = i;
            }
        }
    }

    return bestIndex;
}

  public void ReactivateFromDetail()
  {
    currentDetailPanel = null;      // 현재 열린 패널 해제
    ActivateSelection();            // 입력 다시 켜기
  }
}