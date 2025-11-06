using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class WeldingRobot : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 8f;
    public float viewAngle = 60f;
    public LayerMask visibilityMask = ~0;

    [Header("Rotation")]
    public bool rotateTowardTarget = true;
    public float turnSpeed = 240f;

    [Header("Attack")]
    public float damagePerSecond = 16f;
    public float fireRate = 12f;
    public float projectileSpeed = 10f;
    public float projectileRadius = 0.4f;
    public float maxRange = 7f;
    public float overlapDpsDivider = 3f;

    [Header("References")]
    public Transform nozzle;
    [Tooltip("🔥 발사 이펙트 프리팹 (예: VFX_Fire_01_Big)")]
    public GameObject firePrefab;

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
        GameObject go = new GameObject("FlameProjectile");
        go.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(dir, Vector3.up));
        var proj = go.AddComponent<FlameProjectile>();

        // 🔥 핵심: 불 프리팹 연결
        proj.fireVFXPrefab = firePrefab;

        proj.Initialize(
            speed: projectileSpeed,
            lifeDistance: maxRange,
            radius: projectileRadius,
            dps: damagePerSecond / Mathf.Max(1f, overlapDpsDivider)
        );
    }

    bool HasLineOfSight(Transform originTf, Transform targetTf)
    {
        Vector3 origin = originTf.position;
        Vector3 target = targetTf.position + Vector3.up * 0.9f;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);
        origin += dir * 0.02f;

        var hits = Physics.RaycastAll(origin, dir, dist, visibilityMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider.transform.IsChildOf(transform)) continue;
            return h.collider.GetComponentInParent<PlayerController>() != null;
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Transform refT = nozzle != null ? nozzle : transform;
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Vector3 left = Quaternion.Euler(0, -viewAngle, 0) * refT.forward;
        Vector3 right = Quaternion.Euler(0, +viewAngle, 0) * refT.forward;
        Gizmos.DrawLine(refT.position, refT.position + left * detectionRadius);
        Gizmos.DrawLine(refT.position, refT.position + right * detectionRadius);
    }
}
