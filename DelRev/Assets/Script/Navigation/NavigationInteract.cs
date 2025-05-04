using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationInteract : MonoBehaviour
{
  public GameObject navigationUI;
  public Camera mainCamera;
  public Camera navigationCamera;

  public PlayerController playerController;
  public Inventory inventory;
  public CrossHair crossHair;

  public NavigationButtonSelector buttonSelector;
  public NavigationPanelManager panelManager;

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

    // 카메라 전환
    mainCamera.enabled = false;
    navigationCamera.enabled = true;

    // UI, 커서, 조작
    navigationUI.SetActive(true);
    playerController.isLocked = true;

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    inventory.isInputLocked = true;
    crossHair.interactionLocked = true;

    panelManager.ShowMainPanel(true);
  }

  void EndInteraction()
  {
    isInteracting = false;

    // 카메라 복원
    mainCamera.enabled = true;
    navigationCamera.enabled = false;

    // UI, 조작 복원
    // navigationUI.SetActive(false);
    playerController.isLocked = false;

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    inventory.isInputLocked = false;
    crossHair.interactionLocked = false;

    buttonSelector.DeactivateSelection();
  }
}

