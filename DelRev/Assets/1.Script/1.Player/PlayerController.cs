using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float health = 100f;

    [Header("Movement Settings")]
    public float walkSpeed = 1f;
    public float jumpHeight = 0.3f;
    public float gravity = -9.81f;

    [Tooltip("Shift 키 누를 때 몇 배 빨라질지")]
    public float highSpeedMultiplier = 1.3f;

    [Tooltip("Ctrl 키 누를 때 몇 배 느려질지")]
    public float slowSpeedMultiplier = 0.5f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDecreaseRate = 20f;
    public float staminaRecoveryRate = 10f;

    [Header("Controls")]
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;

    [Header("Runtime Debug")]
    public float stamina;
    public float currentSpeed;

    [Header("Coin System")]
    [Tooltip("플레이어가 획득한 코인(금액) 총합")]
    public int coinCount = 0;

    [Header("Control Lock")]
    public bool isLocked = false;

    private float xRotation = 0f;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isRunning;
    private bool exhausted = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;

        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (isLocked) return;

        HandleMouseLook();
        HandleMovement();
        HandleStamina();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        Vector3 move = transform.right * Input.GetAxis("Horizontal")
                     + transform.forward * Input.GetAxis("Vertical");
        move.Normalize();

        currentSpeed = walkSpeed;
        isCrouching = Input.GetKey(crouchKey);
        if (isCrouching)
            currentSpeed *= slowSpeedMultiplier;

        bool canRun = Input.GetKey(runKey) && !isCrouching
                      && move != Vector3.zero && !exhausted;
        isRunning = canRun;
        if (canRun)
            currentSpeed *= highSpeedMultiplier;

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetKeyDown(jumpKey) && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleStamina()
    {
        if (isRunning)
        {
            stamina -= staminaDecreaseRate * Time.deltaTime;
            if (stamina <= 0f)
            {
                stamina = 0f;
                exhausted = true;
            }
        }
        else
        {
            stamina += staminaRecoveryRate * Time.deltaTime;
            if (exhausted && stamina >= 20f)
                exhausted = false;
        }
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }

    public void AddCoins(int amount)
    {
        coinCount += amount;
        Debug.Log($"[PlayerController] AddCoins: +{amount}, Total = {coinCount}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            AddCoins(1);
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Item"))
        {
            var item = other.GetComponent<Item>();
            if (item != null)
            {
                AddCoins(item.itemPrice);
                Destroy(other.gameObject);
            }
        }
    }
}
