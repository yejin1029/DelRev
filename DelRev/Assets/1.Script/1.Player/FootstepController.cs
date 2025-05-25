using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FootstepController : MonoBehaviour
{
    [Header("Child AudioSources")]
    [Tooltip("발소리용 AudioSource들 (Child로 만든 5개)")]
    public AudioSource[] footstepSources = new AudioSource[5];

    [Header("Motion Settings")]
    [Tooltip("이 이하 속도에서는 발소리를 재생하지 않습니다.")]
    public float minSpeedThreshold = 0.1f;

    [Tooltip("속도 구간 기준들 (오름차순). 예: [0.1, 2.5, 5]")]
    public float[] speedThresholds = new float[]{ 0.1f, 2.5f, 5f };

    [Tooltip("각 구간별 재생 간격(초). thresholds[i] ≤ speed < thresholds[i+1]일 때 intervals[i] 사용")]
    public float[] intervals       = new float[]{ 1f, 0.5f, 0.3f };

    private CharacterController cc;
    private Coroutine          stepLoop;
    private Vector3            lastPosition;
    private float              currentSpeed;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        lastPosition = transform.position;

        // Child AudioSource 기본 세팅: PlayOnAwake, Loop 끄기
        foreach (var src in footstepSources)
            if (src != null)
                src.playOnAwake = src.loop = false;
    }

    void Update()
    {
        // 1) 위치 기반 속도 계산
        Vector3 delta = transform.position - lastPosition;
        currentSpeed = delta.magnitude / Time.deltaTime;
        lastPosition = transform.position;

        bool isMoving   = currentSpeed > minSpeedThreshold;
        bool isGrounded = cc.isGrounded;
        bool isJumping  = Input.GetKeyDown(KeyCode.Space);

        // 점프하면 즉시 코루틴 중단
        if (isJumping && stepLoop != null)
        {
            StopCoroutine(stepLoop);
            stepLoop = null;
        }

        // 걷거나 달릴 때만 코루틴 시작
        if (isMoving && isGrounded && stepLoop == null)
        {
            stepLoop = StartCoroutine(FootstepLoop());
        }
        // 멈추거나 공중에 뜨면 중단
        else if ((!isMoving || !isGrounded) && stepLoop != null)
        {
            StopCoroutine(stepLoop);
            stepLoop = null;
        }
    }

    private System.Collections.IEnumerator FootstepLoop()
    {
        while (true)
        {
            PlayFootstep();
            float interval = GetIntervalForSpeed(currentSpeed);
            yield return new WaitForSeconds(interval);
        }
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

        // 1) 재생 중인 모든 소리 멈추기
        foreach (var s in footstepSources)
        {
            if (s.isPlaying)
                s.Stop();
        }

        // 2) 5개 중 하나를 랜덤으로 골라 재생
        var src = footstepSources[Random.Range(0, footstepSources.Length)];
        if (src != null && src.clip != null)
        {
            src.pitch = Random.Range(0.95f, 1.05f);
            src.Play();
        }
    }
}
