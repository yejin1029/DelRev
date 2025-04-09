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
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
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
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move.Normalize();

        currentSpeed = walkSpeed;

        isCrouching = Input.GetKey(crouchKey);
        if (isCrouching)
        {
            currentSpeed *= slowSpeedMultiplier;
        }

        bool canRun = Input.GetKey(runKey) && !isCrouching && move != Vector3.zero && !exhausted;
        isRunning = canRun;

        if (canRun)
        {
            currentSpeed *= highSpeedMultiplier;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

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
            {
                exhausted = false;
            }
        }

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }
}
