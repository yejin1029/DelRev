using UnityEngine;

public class WeldingRobot : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 8f;
    public float viewAngle = 60f;
    [Tooltip("시야 판정에 포함할 레이어(플레이어 + 장애물 모두 포함)")]
    public LayerMask visibilityMask = ~0; // Everything 권장

    [Header("Rotation")]
    public bool rotateTowardTarget = true;
    public float turnSpeed = 240f;

    [Header("Attack")]
    [Tooltip("플레이어가 불줄기에 닿아 있을 때 기대 총 DPS")]
    public float damagePerSecond = 16f;
    [Tooltip("초당 발사 개수(값이 높을수록 연속 분사 느낌)")]
    public float fireRate = 12f;
    [Tooltip("프로젝트타일 이동 속도")]
    public float projectileSpeed = 10f;
    [Tooltip("프로젝트타일 반지름(히트 폭)")]
    public float projectileRadius = 0.4f;
    [Tooltip("최대 비행 거리")]
    public float maxRange = 7f;
    [Tooltip("겹침 보정(여러 발이 겹칠 때 총합 DPS가 과해지지 않도록)")]
    public float overlapDpsDivider = 3f;

    [Header("References")]
    [Tooltip("노즐(없으면 본체 기준), Z+가 발사 방향")]
    public Transform nozzle;

    bool isSpraying;
    float fireTimer;

    void Start()
    {
        if (nozzle == null) nozzle = transform;
    }

    void Update()
    {
        var player = PlayerController.Instance;
        if (player == null) return;

        Transform refT = nozzle != null ? nozzle : transform;
        Vector3 playerAimPos = player.transform.position + Vector3.up * 0.9f;
        Vector3 toPlayer = playerAimPos - refT.position;

        float dist = toPlayer.magnitude;
        bool inRange = dist <= detectionRadius;

        bool inFOV = false;
        if (inRange)
        {
            float angle = Vector3.Angle(refT.forward, toPlayer);
            inFOV = angle <= viewAngle;
        }

        bool hasLOS = false;
        if (inRange && inFOV)
        {
            hasLOS = HasLineOfSight(refT, player.transform);
        }

        bool shouldSpray = inRange && inFOV && hasLOS;

        if (rotateTowardTarget && (inRange || isSpraying))
        {
            Vector3 flat = toPlayer; flat.y = 0f;
            if (flat.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flat.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }

        if (shouldSpray)
        {
            if (!isSpraying)
            {
                isSpraying = true;
                fireTimer = 0f;
            }

            fireTimer += Time.deltaTime;
            float interval = 1f / Mathf.Max(0.01f, fireRate);
            while (fireTimer >= interval)
            {
                fireTimer -= interval;

                // 스냅샷 조준(발사 시점의 플레이어 위치)
                Vector3 targetPoint = player.transform.position + Vector3.up * 0.9f;
                Vector3 dir = (targetPoint - refT.position).normalized;
                FireOne(refT.position, dir);
            }
        }
        else
        {
            if (isSpraying) isSpraying = false;
        }
    }

    void FireOne(Vector3 origin, Vector3 dir)
    {
        // 프리팹 없이 코드로 즉석 생성
        GameObject go = new GameObject("FlameProjectile");
        go.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(dir, Vector3.up));
        var proj = go.AddComponent<FlameProjectile>();

        // 파라미터 전달
        proj.Initialize(
            speed: projectileSpeed,
            lifeDistance: maxRange,
            radius: projectileRadius,
            dps: damagePerSecond / Mathf.Max(1f, overlapDpsDivider)
        );
    }

    // === LOS 유틸 ===
    bool HasLineOfSight(Transform originTf, Transform targetTf)
    {
        Vector3 origin = originTf.position;
        Vector3 target = targetTf.position + Vector3.up * 0.9f;
        Vector3 dir    = (target - origin).normalized;
        float   dist   = Vector3.Distance(origin, target);
        origin += dir * 0.02f; // 자기 콜라이더 내부 시작 보정

        var hits = Physics.RaycastAll(origin, dir, dist, visibilityMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider.transform.IsChildOf(transform)) continue; // 자기 자신 무시
            return h.collider.GetComponentInParent<PlayerController>() != null;
        }
        return true; // 히트가 없으면 막힘 없음으로 간주
    }

    void OnDrawGizmosSelected()
    {
        Transform refT = nozzle != null ? nozzle : transform;

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Vector3 left  = Quaternion.Euler(0, -viewAngle, 0) * refT.forward;
        Vector3 right = Quaternion.Euler(0, +viewAngle, 0) * refT.forward;
        Gizmos.DrawLine(refT.position, refT.position + left  * detectionRadius);
        Gizmos.DrawLine(refT.position, refT.position + right * detectionRadius);
    }
}
