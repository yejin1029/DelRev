using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailPanelSelector : MonoBehaviour
{
    [Header("버튼 및 선택 테두리")]
    public Button[] buttons;               
    public GameObject[] selectionOutlines;

    [Header("씬 이동 관련")]
    public SceneChanger sceneChanger;
    public string sceneNameToLoad;

    [Header("연결된 시스템")]
    public MapLocationSelector mapSelector;

    [Header("🔊 사운드")]
    public AudioSource audioSource;
    public AudioClip selectSfx;   // 선택 이동 사운드
    public AudioClip submitSfx;   // 클릭(확정) 사운드

    private int selectedIndex = 0;
    private bool isActive = false;
    private bool isTransitioning = false;

    void Awake()
    {
        if (buttons != null && buttons.Length > 0)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int idx = i;
                buttons[i].onClick.AddListener(() => OnClickIndex(idx));
            }
        }
    }

    public void Activate()
    {
        isActive = true;
        isTransitioning = false;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, buttons.Length - 1);

        HideOutlines();
        UpdateVisuals();
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        isActive = false;
        HideOutlines();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isActive || isTransitioning) return;

        int before = selectedIndex;

        // 좌우 이동
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            selectedIndex = Mathf.Max(0, selectedIndex - 1);

        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            selectedIndex = Mathf.Min(buttons.Length - 1, selectedIndex + 1);

        // 🔊 선택 이동 사운드
        if (before != selectedIndex)
        {
            PlaySelectSound();
            UpdateVisuals();
        }

        // 엔터키 → 확정
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            PlaySubmitSound();                    // 🔊 클릭 사운드
            buttons[selectedIndex].onClick.Invoke(); // 버튼 onClick 실행
            SubmitSelection();                      // 기존 기능 유지
        }

        // ESC → 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    private void SubmitSelection()
    {
        if (selectedIndex == 0)
            LoadSceneFromDetail();
        else
            ClosePanel();
    }

    private void OnClickIndex(int index)
    {
        if (!isActive || isTransitioning) return;

        selectedIndex = index;

        PlaySubmitSound();     // 🔊 마우스 클릭 사운드  
        UpdateVisuals();
        SubmitSelection();
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < selectionOutlines.Length; i++)
            selectionOutlines[i]?.SetActive(i == selectedIndex);
    }

    void HideOutlines()
    {
        foreach (var o in selectionOutlines)
            o?.SetActive(false);
    }

    public void LoadSceneFromDetail()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (sceneChanger != null)
        {
            isActive = false;
            HideOutlines();
            gameObject.SetActive(false);
            sceneChanger.ChangeScene(sceneNameToLoad);
        }
        else
        {
            isTransitioning = false;
        }
    }

    public void ClosePanel()
    {
        if (isTransitioning) return;

        isActive = false;
        HideOutlines();
        gameObject.SetActive(false);

        mapSelector?.ReactivateFromDetail();
    }

    // 🔊 사운드 재생 함수들
    void PlaySelectSound()
    {
        if (audioSource != null && selectSfx != null)
            audioSource.PlayOneShot(selectSfx);
    }

    void PlaySubmitSound()
    {
        if (audioSource != null && submitSfx != null)
            audioSource.PlayOneShot(submitSfx);
    }
}
