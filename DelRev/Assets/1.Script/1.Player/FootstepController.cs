using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FootstepController : MonoBehaviour
{
    [Header("Child AudioSources (e.g. FootSteps/CHR_RockWalk_1~5)")]
    public AudioSource[] footstepSources;

    [Header("Motion Settings")]
    [Tooltip("이 이하 속도에서는 발소리를 재생하지 않습니다.")]
    public float minSpeedThreshold = 0.1f;

    [Tooltip("속도 구간 기준들 (오름차순). 예: [0.1, 2.5, 5]")]
    public float[] speedThresholds = new float[] { 0.1f, 2.5f, 5f };

    [Tooltip("각 구간별 재생 간격(초). thresholds[i] ≤ speed < thresholds[i+1]일 때 intervals[i] 사용")]
    public float[] intervals = new float[] { 1f, 0.5f, 0.3f };

    [Header("Audio Settings")]
    [Tooltip("같은 소리 연속 재생 방지")]
    public bool avoidRepeat = true;

    [Header("Camera Shake Settings")]
    [Tooltip("카메라 흔들림 강도 (기본 0.05 = 5cm)")]
    public float shakeIntensity = 0.05f;
    [Tooltip("카메라가 원래 위치로 돌아오는 속도")]
    public float shakeReturnSpeed = 5f;

    private CharacterController cc;
    private Vector3 lastPosition;
    private float currentSpeed;
    private float stepTimer;
    private int lastPlayedIndex = -1;

    // 🔸 카메라 흔들림 관련
    private Transform camTransform;
    private Vector3 camDefaultLocalPos;
    private float shakeOffsetY = 0f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        lastPosition = transform.position;

        // 🔸 카메라 찾기 (Player 하위 Main Camera 자동 탐색)
        camTransform = GetComponentInChildren<Camera>()?.transform;
        if (camTransform != null)
            camDefaultLocalPos = camTransform.localPosition;

        // Child AudioSource 기본 세팅
        foreach (var src in footstepSources)
        {
            if (src != null)
            {
                src.playOnAwake = false;
                src.loop = false;
                src.spatialBlend = 0f; // 2D 사운드
            }
        }
    }

    void Update()
    {
        // 속도 계산
        Vector3 delta = transform.position - lastPosition;
        currentSpeed = delta.magnitude / Time.deltaTime;
        lastPosition = transform.position;

        bool isMoving = currentSpeed > minSpeedThreshold;
        bool isGrounded = cc.isGrounded;

        if (isMoving && isGrounded)
        {
            float interval = GetIntervalForSpeed(currentSpeed);
            stepTimer += Time.deltaTime;

            if (stepTimer >= interval)
            {
                PlayFootstep();
                TriggerCameraShake();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }

        UpdateCameraShake();
    }

    private float GetIntervalForSpeed(float speed)
    {
        int len = Mathf.Min(speedThresholds.Length, intervals.Length);
        for (int i = len - 1; i >= 0; i--)
        {
            if (speed >= speedThresholds[i])
                return intervals[i];
        }
        return intervals[0];
    }

    private void PlayFootstep()
    {
        if (footstepSources == null || footstepSources.Length == 0)
            return;

        int index;
        do
        {
            index = Random.Range(0, footstepSources.Length);
        }
        while (avoidRepeat && footstepSources.Length > 1 && index == lastPlayedIndex);

        lastPlayedIndex = index;
        var src = footstepSources[index];

        if (src != null && src.clip != null)
        {
            src.pitch = Random.Range(0.95f, 1.05f);
            src.PlayOneShot(src.clip);
        }
    }

    // 🔸 발소리 시점에 카메라 살짝 흔들기
    private void TriggerCameraShake()
    {
        if (camTransform == null) return;

        float speedFactor = Mathf.Clamp01(currentSpeed / (speedThresholds.Length > 0 ? speedThresholds[^1] : 5f));
        float intensity = shakeIntensity * Mathf.Lerp(0.5f, 1.5f, speedFactor);
        shakeOffsetY = intensity;
    }

    // 🔸 흔들림을 자연스럽게 되돌림
    private void UpdateCameraShake()
    {
        if (camTransform == null) return;

        shakeOffsetY = Mathf.Lerp(shakeOffsetY, 0f, Time.deltaTime * shakeReturnSpeed);
        camTransform.localPosition = camDefaultLocalPos + new Vector3(0f, Mathf.Sin(Time.time * 20f) * shakeOffsetY, 0f);
    }
}
