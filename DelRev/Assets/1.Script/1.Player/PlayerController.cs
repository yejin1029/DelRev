using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float health = 100f;

    [Header("Movement Settings")]
    public float walkSpeed = 1f;
    public float jumpHeight = 0.3f;
    public float gravity = -9.81f;
    public float highSpeedMultiplier = 1.3f;
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
    public int coinCount = 0;

    [Header("Control Lock")]
    public bool isLocked = false;

    [Header("UI References")]
    public Image staminaImage;
    public Sprite[] staminaSprites;
    public Image healthFillImage;
    public CoinUI coinUI;

    private float xRotation = 0f;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isRunning;
    private bool exhausted = false;
    private bool isDead = false;

    private Color originalHealthColor;
    private Coroutine healthFlashCoroutine;

    private void Awake()
    {
        Debug.Log("[PlayerController] 초기화 중...");

        health = 100f;
        stamina = maxStamina;
        isDead = false;
        isLocked = false;
        exhausted = false;

        Transform spawn = GameObject.FindWithTag("SpawnPoint")?.transform;
        if (spawn != null)
            transform.position = spawn.position;
        else
            transform.position = new Vector3(0, 1, 0);

        xRotation = 0f;
        transform.rotation = Quaternion.identity;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;

        if (healthFillImage != null)
            originalHealthColor = healthFillImage.color;

        if (health <= 0f)
        {
            Die();
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetStats();
    }

    public void ResetStats()
    {
        health = 100f;
        stamina = maxStamina;
        isDead = false;
        isLocked = false;
        exhausted = false;

        UpdateHealthUI();
        UpdateStaminaUI();

        Debug.Log("[PlayerController] 씬 이동 감지 → 체력 및 스태미나 초기화");
    }

    private void Update()
    {
        if (!isDead && health <= 0f)
        {
            Die();
            return;
        }

        if (isLocked || isDead) return;

        HandleMouseLook();
        HandleMovement();
        HandleStamina();
        UpdateStaminaUI();
        UpdateHealthUI();
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

        Vector3 move = transform.right * Input.GetAxis("Horizontal") +
                       transform.forward * Input.GetAxis("Vertical");
        move.Normalize();

        currentSpeed = walkSpeed;
        isCrouching = Input.GetKey(crouchKey);
        if (isCrouching)
            currentSpeed *= slowSpeedMultiplier;

        bool canRun = Input.GetKey(runKey) && !isCrouching && move != Vector3.zero && !exhausted;
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

        if (coinUI == null)
            coinUI = FindObjectOfType<CoinUI>();

        if (coinUI != null)
            coinUI.UpdateCoinText(coinCount);
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

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        health = Mathf.Clamp(health, 0f, 100f);

        UpdateHealthUI();
        FlashHealthUI();

        if (health <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[PlayerController] Player died. Loading GameOver scene...");
        isLocked = true;
        SceneManager.LoadScene("GameOver");
    }

    void FlashHealthUI()
    {
        if (healthFillImage == null) return;

        if (healthFlashCoroutine != null)
            StopCoroutine(healthFlashCoroutine);

        healthFlashCoroutine = StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        healthFillImage.material = null;
        healthFillImage.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        healthFillImage.color = originalHealthColor;
    }

    void UpdateStaminaUI()
    {
        if (staminaSprites == null || staminaSprites.Length == 0 || staminaImage == null)
            return;

        float staminaRatio = stamina / maxStamina;
        int index = Mathf.Clamp(
            Mathf.FloorToInt(staminaRatio * (staminaSprites.Length - 1)),
            0, staminaSprites.Length - 1
        );

        staminaImage.sprite = staminaSprites[index];
    }

    void UpdateHealthUI()
    {
        if (healthFillImage != null)
        {
            float healthRatio = health / 100f;
            healthFillImage.fillAmount = Mathf.Clamp01(healthRatio);
        }
    }
}
