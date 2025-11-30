using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationInteract : MonoBehaviour
{
    [Header("UI & Camera")]
    public GameObject navigationUI;
    public Camera mainCamera;
    public Camera navigationCamera;

    [Header("Player Control")]
    public PlayerController playerController;
    public Inventory inventory;
    public CrossHair crossHair;
    public GameObject crossHairUI;

    [Header("Navigation")]
    public NavigationButtonSelector buttonSelector;
    public NavigationPanelManager panelManager;

    [Header("Sound")]
    public AudioSource audioSource;          // 재생할 AudioSource
    public AudioClip enterNavigationSound;   // 네비게이션 진입 시 재생할 사운드

    private bool isInteracting = false;

    void Start()
    {
        navigationCamera.enabled = false;
    }

    void Update()
    {
        if (!isInteracting && Input.GetKeyDown(KeyCode.E) && crossHair.isAimingAtNavigation)
        {
            StartInteraction();
        }
        else if (isInteracting && Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelManager.IsMainPanelActive())
            {
                EndInteraction();
            }
        }
    }

    void StartInteraction()
    {
        isInteracting = true;

        // 🔊 네비게이션 진입 소리 재생
        if (audioSource != null && enterNavigationSound != null)
        {
            audioSource.PlayOneShot(enterNavigationSound);
        }

        // 카메라 전환
        mainCamera.enabled = false;
        navigationCamera.enabled = true;

        // UI 활성화 / 입력 락
        navigationUI.SetActive(true);
        playerController.isLocked = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        inventory.isInputLocked = true;
        crossHair.interactionLocked = true;

        // 조준점 숨기기
        if (crossHairUI != null)
            crossHairUI.SetActive(false);

        panelManager.ShowMainPanel(true);
    }

    void EndInteraction()
    {
        isInteracting = false;

        // 카메라 복구
        mainCamera.enabled = true;
        navigationCamera.enabled = false;

        // UI 복구 / 입력 복원
        // navigationUI.SetActive(false);
        playerController.isLocked = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inventory.isInputLocked = false;
        crossHair.interactionLocked = false;

        // 조준점 복원
        if (crossHairUI != null)
            crossHairUI.SetActive(true);

        buttonSelector.DeactivateSelection();
    }
}
