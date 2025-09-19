using UnityEngine;

public class WeldingRobot : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("플레이어를 감지할 최대 반경")]
    public float detectionRadius = 6f;
    [Tooltip("정면 기준 공격 가능한 좌우 각도(도 단위)")]
    public float viewAngle = 45f;
    [Tooltip("장애물 레이어(시야 판정용)")]
    public LayerMask visibilityMask = ~0; // Everything

    [Header("Rotation")]
    [Tooltip("플레이어를 향해 회전할지 여부(자리 이동 없음)")]
    public bool rotateTowardTarget = true;
    [Tooltip("회전 속도(도/초)")]
    public float turnSpeed = 240f;

    [Header("Attack (Gameplay)")]
    [Tooltip("초당 대미지(DPS)")]
    public float damagePerSecond = 15f;
    [Tooltip("불길 사거리 (히트박스/가시효과 길이)")]
    public float flameRange = 4f;
    [Tooltip("불길 반폭(가로 폭 절반, 히트박스/가시효과 폭)")]
    public float flameHalfWidth = 0.6f;

    [Header("Attack (VFX Tuning)")]
    [Tooltip("분사 속도(파티클 속도/간접적으로 길이와 사세 조절)")]
    public float spraySpeed = 8f;
    [Tooltip("분사 입자량(초당 방출량)")]
    public float emissionRate = 120f;

    [Header("References")]
    [Tooltip("노즐 기준(정면 Z+ 방향)")]
    public Transform nozzle;
    [Tooltip("불꽃 파티클(선택)")]
    public ParticleSystem flameVFX;
    [Tooltip("분사 히트박스(Trigger Collider)")]
    public Collider flameHitbox; // BoxCollider 권장 (isTrigger = true)

    bool isSpraying;

    void Start()
    {
        if (flameHitbox != null) flameHitbox.enabled = false;
        if (flameVFX != null) flameVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ApplyHitboxSize();
        ApplyVFXParams();
    }

    void Update()
    {
        var player = PlayerController.Instance;
        if (player == null) return;

        // 1) 거리 체크
        Vector3 toPlayer = (player.transform.position + Vector3.up * 0.9f) - (nozzle ? nozzle.position : transform.position);
        float dist = toPlayer.magnitude;
        bool inRange = dist <= detectionRadius;

        // 2) 시야 각(FOV) 체크
        bool inFOV = false;
        if (inRange)
        {
            Transform refT = nozzle != null ? nozzle : transform;
            float angle = Vector3.Angle(refT.forward, toPlayer);
            inFOV = angle <= viewAngle;
        }

        // 3) 시야 막힘(LOS) 체크
        bool hasLOS = false;
        if (inRange && inFOV)
        {
            Vector3 origin = nozzle ? nozzle.position : transform.position;
            Vector3 dir = toPlayer.normalized;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, detectionRadius + 1f, visibilityMask, QueryTriggerInteraction.Ignore))
            {
                // 첫 맞춘 것이 Player면 LOS 통과, 그 외(벽/지형 등)면 실패
                hasLOS = hit.collider.GetComponentInParent<PlayerController>() != null;
            }
        }

        bool shouldSpray = inRange && inFOV && hasLOS;

        // 제자리 회전(선택)
        if (rotateTowardTarget && (inRange || isSpraying))
        {
            Vector3 flat = toPlayer; flat.y = 0f;
            if (flat.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flat.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }

        // 분사 토글
        if (shouldSpray && !isSpraying) StartSpray();
        else if (!shouldSpray && isSpraying) StopSpray();

        // 분사 중일 때, 히트박스가 OnTriggerStay로 대미지 처리
        // (여기서는 별도 처리 없음)

        Debug.Log($"range:{inRange}, fov:{inFOV}, los:{hasLOS}, should:{shouldSpray}");
    }

    void StartSpray()
    {
        Debug.Log("분사 시작");
        isSpraying = true;
        if (flameHitbox != null) flameHitbox.enabled = true;
        if (flameVFX != null) flameVFX.Play();
    }

    void StopSpray()
    {
        isSpraying = false;
        if (flameHitbox != null) flameHitbox.enabled = false;
        if (flameVFX != null) flameVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    // 파티클 파라미터를 public 슬라이더로 제어
    void ApplyVFXParams()
    {
        if (flameVFX == null) return;

        var main = flameVFX.main;
        main.startSpeed = spraySpeed;

        var emission = flameVFX.emission;
        emission.rateOverTime = emissionRate;

        var shape = flameVFX.shape;
        // 반폭을 파티클 원뿔/원 형태에 맞춰 대략 반영
        shape.radius = Mathf.Max(0.05f, flameHalfWidth);
    }

    // BoxCollider 히트박스 크기 자동 세팅(노즐 정면 Z+ 기준)
    void ApplyHitboxSize()
    {
        if (flameHitbox == null) return;

        var box = flameHitbox as BoxCollider;
        if (box != null)
        {
            box.isTrigger = true;
            box.center = new Vector3(0f, 0.5f, flameRange * 0.5f);
            box.size   = new Vector3(flameHalfWidth * 2f, flameHalfWidth * 1.2f, flameRange);
        }
    }

    void OnValidate()
    {
        flameRange = Mathf.Max(0.1f, flameRange);
        flameHalfWidth = Mathf.Max(0.05f, flameHalfWidth);
        detectionRadius = Mathf.Max(flameRange, detectionRadius); // 실수 방지
        ApplyHitboxSize();
        ApplyVFXParams();
    }

    void OnDrawGizmosSelected()
    {
        Transform refT = nozzle != null ? nozzle : transform;

        // 감지 반경
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // FOV 시각화
        Vector3 left  = Quaternion.Euler(0, -viewAngle, 0) * refT.forward;
        Vector3 right = Quaternion.Euler(0, +viewAngle, 0) * refT.forward;
        Gizmos.DrawLine(refT.position, refT.position + left  * detectionRadius);
        Gizmos.DrawLine(refT.position, refT.position + right * detectionRadius);

        // 불길 히트박스
        Gizmos.color = new Color(1f, 0.2f, 0f, 0.25f);
        Matrix4x4 m = Gizmos.matrix;
        Gizmos.matrix = refT.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(0, 0.5f, flameRange * 0.5f),
                            new Vector3(flameHalfWidth * 2f, flameHalfWidth * 1.2f, flameRange));
        Gizmos.matrix = m;
    }
}
