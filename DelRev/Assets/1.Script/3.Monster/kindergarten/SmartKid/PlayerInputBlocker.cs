using UnityEngine;

public class PlayerInputBlocker : MonoBehaviour
{
    private bool isBlocked = false;
    private PlayerController playerController;

    public bool IsBlocked => isBlocked;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void BlockInput()
    {
        isBlocked = true;
        if (playerController != null)
            playerController.enabled = false;

        Debug.Log("[PlayerInputBlocker] 입력 차단됨");
    }

    public void UnblockInput()
    {
        isBlocked = false;
        if (playerController != null)
            playerController.enabled = true;

        Debug.Log("[PlayerInputBlocker] 입력 복구됨");
    }
}
