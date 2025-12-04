using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Damage : MonoBehaviour
{
    public static Damage Instance;

    [Header("Camera Shake Settings")]
    public Transform cameraTransform;
    public float defaultDuration = 0.2f;
    public float defaultMagnitude = 0.1f;

    [Header("Sound Settings")]
    public AudioSource audioSource;
    public AudioClip damageSound;
    [Range(0f, 1f)] public float soundVolume = 0.6f;
    public float soundCooldown = 0.3f;
    private float lastSoundTime = -1f;

    [Header("UI Flash Effect")]
    public CanvasGroup redFlashCanvas;
    public float transitionSpeed = 2f;
    public float maxHealthAlpha = 0.7f;
    public float minHealthAlpha = 0.0f;

    private Vector3 originalCamPos;
    private Coroutine shakeCoroutine;
    private float targetAlpha = 0f;

    // 🔥 체력 감소 감지를 위한 변수
    private float previousHealth = 100f;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (cameraTransform != null)
            originalCamPos = cameraTransform.localPosition;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        audioSource.volume = soundVolume;

        if (redFlashCanvas != null)
            redFlashCanvas.alpha = 0f;

        // 플레이어 체력 초기값 가져오기
        if (PlayerController.Instance != null)
            previousHealth = PlayerController.Instance.health;
    }

    private void Update()
    {
        // 🔴 체력 비례 화면 붉은 효과
        if (PlayerController.Instance != null && redFlashCanvas != null)
        {
            float healthRatio = PlayerController.Instance.health / 100f;
            targetAlpha = Mathf.Lerp(maxHealthAlpha, minHealthAlpha, healthRatio);
            redFlashCanvas.alpha = Mathf.Lerp(redFlashCanvas.alpha, targetAlpha, Time.deltaTime * transitionSpeed);
        }

        // 🔥 체력 감소 자동 감지 → 카메라 흔들림 실행
        if (PlayerController.Instance != null)
        {
            float currentHealth = PlayerController.Instance.health;

            if (currentHealth < previousHealth)
            {
                float lostAmount = previousHealth - currentHealth;
                TriggerDamageEffect(lostAmount);
            }

            previousHealth = currentHealth;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 메인 카메라 다시 찾기
        if (cameraTransform == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cameraTransform = cam.transform;
                originalCamPos = cameraTransform.localPosition;
            }
        }

        // DamageFlash 캔버스 자동 연결
        if (redFlashCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("DamageFlash");
            if (canvasObj != null)
                redFlashCanvas = canvasObj.GetComponent<CanvasGroup>();
        }

        // 씬이 바뀌면 체력 초기화
        if (PlayerController.Instance != null)
            previousHealth = PlayerController.Instance.health;
    }

    public void TriggerDamageEffect(float damageAmount)
    {
        if (cameraTransform == null) return;

        // 🔊 쿨타임 체크 후 데미지 사운드 재생
        if (damageSound != null && Time.time - lastSoundTime >= soundCooldown)
        {
            audioSource.PlayOneShot(damageSound, soundVolume);
            lastSoundTime = Time.time;
        }

        // 📸 데미지 강도 비례 카메라 흔들림
        float shakeMagnitude = Mathf.Lerp(0.05f, 0.2f, damageAmount / 100f);
        StartShake(defaultDuration, shakeMagnitude);
    }

    private void StartShake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (cameraTransform == null) yield break;

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cameraTransform.localPosition = originalCamPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (cameraTransform != null)
            cameraTransform.localPosition = originalCamPos;
    }
}
