using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailPanelSelector : MonoBehaviour
{
  [Header("버튼 및 선택 테두리")]
  public Button[] buttons;               // 선택할 버튼 2개
  public GameObject[] selectionOutlines; // 버튼 위의 강조 테두리

  [Header("씬 이동 관련")]
  public SceneChanger sceneChanger; // 외부 SceneChanger 스크립트 참조
  public string sceneNameToLoad;    // 이동할 씬 이름

  [Header("연결된 시스템")]
  public MapLocationSelector mapSelector;

  private int selectedIndex = 0;
  private bool isActive = false;

  // 패널이 열릴 때 호출됨
  public void Activate()
  {
    isActive = true;
    selectedIndex = 0;
    HideOutlines();
    UpdateVisuals();
  }

  // 패널이 닫힐 때 호출됨
  public void Deactivate()
  {
    isActive = false;
    HideOutlines();
  }

  void Update()
  {
    if (!isActive) return;

    // 좌우 입력
    if (Input.GetKeyDown(KeyCode.A))
    {
      selectedIndex = Mathf.Max(0, selectedIndex - 1);
      UpdateVisuals();
    }
    else if (Input.GetKeyDown(KeyCode.D))
    {
      selectedIndex = Mathf.Min(buttons.Length - 1, selectedIndex + 1);
      UpdateVisuals();
    }

    // 선택 확정 (엔터)
    if (Input.GetKeyDown(KeyCode.Return))
    {
      if (selectedIndex == 0)
        LoadSceneFromDetail(); // 왼쪽 → 씬 이동
      else
        ClosePanel();          // 오른쪽 → 닫기
    }

    // ESC로 패널 닫기
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      ClosePanel();
    }
  }

  void UpdateVisuals()
  {
    for (int i = 0; i < selectionOutlines.Length; i++)
    {
      selectionOutlines[i].SetActive(i == selectedIndex);
    }
  }

  void HideOutlines()
  {
    foreach (var o in selectionOutlines)
    {
      o.SetActive(false);
    }
  }

  public void LoadSceneFromDetail()
  {
    if (sceneChanger != null)
    {
      Debug.Log($"[DetailPanelSelector] 씬 이동: {sceneNameToLoad}");
      sceneChanger.ChangeScene(sceneNameToLoad); // ChangeScene() 호출
    }
    else
    {
      Debug.LogWarning("SceneChanger가 연결되지 않았습니다.");
    }
  }

  public void ClosePanel()
  {
    Debug.Log("[DetailPanelSelector] 패널 닫기");
    gameObject.SetActive(false);

    if (mapSelector != null)
    {
      mapSelector.ReactivateFromDetail();
    }
  }
}
