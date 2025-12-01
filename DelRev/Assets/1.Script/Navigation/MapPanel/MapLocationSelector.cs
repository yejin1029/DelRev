using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapLocationSelector : MonoBehaviour
{
    public Image[] locations;                  
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public NavigationPanelManager panelManager;

    private int selectedIndex = 0;
    private bool isActive = false;

    public GameObject[] detailPanels;
    private GameObject currentDetailPanel;

    // 🔊 추가: 선택 이동 사운드
    public AudioSource audioSource;
    public AudioClip selectSound;


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
            int beforeIndex = selectedIndex;   // 🔥 추가: 이전 선택 저장

            // 방향키 입력
            if (Input.GetKeyDown(KeyCode.W))
                selectedIndex = GetNextByInput(Vector2.up);

            if (Input.GetKeyDown(KeyCode.S))
                selectedIndex = GetNextByInput(Vector2.down);

            if (Input.GetKeyDown(KeyCode.A))
                selectedIndex = GetNextByInput(Vector2.left);

            if (Input.GetKeyDown(KeyCode.D))
                selectedIndex = GetNextByInput(Vector2.right);


            // 🔊 선택이 변경되었으면 사운드 재생
            if (beforeIndex != selectedIndex)
            {
                PlaySelectSound();
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

        // ESC로 돌아가기
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


    // 🔊 사운드 재생 함수
    void PlaySelectSound()
    {
        if (audioSource != null && selectSound != null)
            audioSource.PlayOneShot(selectSound);
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
            case 0:
                if (dir == Vector2.right) return 2;
                if (dir == Vector2.down) return 1;
                break;
            case 1:
                if (dir == Vector2.up) return 0;
                if (dir == Vector2.right) return 2;
                break;
            case 2:
                if (dir == Vector2.left) return 0;
                if (dir == Vector2.right) return 3;
                break;
            case 3:
                if (dir == Vector2.left) return 2;
                if (dir == Vector2.down) return 4;
                break;
            /* 연구소 생기면 추가
            case 4:
                if (dir == Vector2.up) return 3;
                if (dir == Vector2.left) return 2;
                break;
            */
        }
        return selectedIndex;
    }

    public void ReactivateFromDetail()
    {
        currentDetailPanel = null;
        ActivateSelection();
    }
}
