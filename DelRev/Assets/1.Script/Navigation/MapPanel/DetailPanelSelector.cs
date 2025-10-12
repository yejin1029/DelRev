using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailPanelSelector : MonoBehaviour
{
    [Header("버튼 및 선택 테두리")]
    [Tooltip("선택 가능한 버튼들 (예: 왼쪽=이동, 오른쪽=닫기)")]
    public Button[] buttons;               // 선택할 버튼 2개
    [Tooltip("각 버튼 위에 올려둘 강조(하이라이트) 오브젝트들")]
    public GameObject[] selectionOutlines; // 버튼 위의 강조 테두리

    [Header("씬 이동 관련")]
    [Tooltip("외부 SceneChanger (내부에서 SceneLoader.Load로 우회)")]
    public SceneChanger sceneChanger; // 외부 SceneChanger 스크립트 참조
    [Tooltip("왼쪽 항목(=이동) 선택 시 전환할 씬 이름")]
    public string sceneNameToLoad;    // 이동할 씬 이름

    [Header("연결된 시스템")]
    public MapLocationSelector mapSelector;

    private int selectedIndex = 0;
    private bool isActive = false;          // 패널 활성화 여부
    private bool isTransitioning = false;   // 씬 전환 중 중복 입력 방지

    void Awake()
    {
        // 배열 길이 매칭 체크(선택 테두리는 버튼 개수와 동일하게 두는 것을 권장)
        if (selectionOutlines != null && buttons != null && selectionOutlines.Length != buttons.Length)
        {
            Debug.LogWarning($"[DetailPanelSelector] selectionOutlines({selectionOutlines.Length}) 길이가 buttons({buttons.Length})와 다릅니다. 인덱스 오류 가능성 있음.");
        }

        // 버튼이 연결되어 있다면 클릭 핸들러를 연결(에디터에서 OnClick을 안 걸어도 동작하도록)
        if (buttons != null && buttons.Length > 0)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int idx = i; // 클로저 캡쳐
                buttons[i].onClick.AddListener(() => OnClickIndex(idx));
            }
        }
    }

    // 패널이 열릴 때 호출됨
    public void Activate()
    {
        isActive = true;
        isTransitioning = false;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, (buttons?.Length ?? 1) - 1));
        HideOutlines();
        UpdateVisuals();
        gameObject.SetActive(true);
    }

    // 패널이 닫힐 때 호출됨
    public void Deactivate()
    {
        isActive = false;
        HideOutlines();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isActive || isTransitioning) return;

        // 좌우 입력 (A/D, 방향키)
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveSelection(+1);
        }

        // 선택 확정 (엔터 / 스페이스)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SubmitSelection();
        }

        // ESC로 패널 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    private void MoveSelection(int delta)
    {
        if (buttons == null || buttons.Length == 0) return;

        int before = selectedIndex;
        selectedIndex = Mathf.Clamp(selectedIndex + delta, 0, buttons.Length - 1);
        if (before != selectedIndex)
        {
            UpdateVisuals();
        }
    }

    private void SubmitSelection()
    {
        // 관례: 0 = 왼쪽(씬 이동), 그 외 = 닫기
        if (selectedIndex == 0)
            LoadSceneFromDetail();
        else
            ClosePanel();
    }

    void UpdateVisuals()
    {
        if (selectionOutlines == null) return;

        for (int i = 0; i < selectionOutlines.Length; i++)
        {
            if (selectionOutlines[i] == null) continue;
            selectionOutlines[i].SetActive(i == selectedIndex);
        }
    }

    void HideOutlines()
    {
        if (selectionOutlines == null) return;

        foreach (var o in selectionOutlines)
        {
            if (o != null) o.SetActive(false);
        }
    }

    /// <summary>
    /// 버튼 클릭 시에도 키보드 제출과 동일한 동작을 보장
    /// </summary>
    private void OnClickIndex(int index)
    {
        if (!isActive || isTransitioning) return;

        selectedIndex = index;
        UpdateVisuals();
        SubmitSelection();
    }

    public void LoadSceneFromDetail()
    {
        if (isTransitioning) return; // 중복 방지
        isTransitioning = true;

        if (sceneChanger != null)
        {
            Debug.Log($"[DetailPanelSelector] 씬 이동 요청: {sceneNameToLoad}");
            // 입력 잠그고 하이라이트 정리
            isActive = false;
            HideOutlines();

            // 패널 닫아주기(시각적으로)
            gameObject.SetActive(false);

            // 로딩씬 경유 전환 (SceneChanger 내부에서 SceneLoader.Load 호출)
            sceneChanger.ChangeScene(sceneNameToLoad);
        }
        else
        {
            isTransitioning = false;
            Debug.LogWarning("[DetailPanelSelector] SceneChanger가 연결되지 않았습니다.");
        }
    }

    public void ClosePanel()
    {
        if (isTransitioning) return;

        Debug.Log("[DetailPanelSelector] 패널 닫기");
        isActive = false;
        HideOutlines();
        gameObject.SetActive(false);

        if (mapSelector != null)
        {
            mapSelector.ReactivateFromDetail();
        }
    }
}
