using UnityEngine;

public class CCTVScanner : MonoBehaviour
{
    public float rotationSpeed = 72f; // 360도 / 5초
    public float viewDistance = 3f;
    public float viewAngle = 120f;
    public float detectTimeThreshold = 3f;

    private Transform player;
    private float playerInSightTimer = 0f;
    private bool hasTriggered = false;

    [Header("Detection Sound")]
    public AudioSource warningAudio;

    [Header("Debug")]
    public bool debugDraw = true;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        RotateCCTV();

        if (player == null) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 시야 범위 조건 체크
        if (distanceToPlayer < viewDistance)
        {
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            if (angle < viewAngle / 2f)
            {
                if (HasLineOfSight())
                {
                    playerInSightTimer += Time.deltaTime;

                    if (playerInSightTimer >= detectTimeThreshold && !hasTriggered)
                    {
                        hasTriggered = true;
                        warningAudio?.Play();

                        // 원장에게 플레이어 위치 전달
                        Principal principal = FindObjectOfType<Principal>();
                        if (principal != null)
                        {
                            principal.AlertToPosition(player.position);
                        }
                    }
                    return;
                }
            }
        }

        // 시야 벗어남
        playerInSightTimer = 0f;
        hasTriggered = false;
    }

    private void RotateCCTV()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f; // CCTV 높이 보정
        Vector3 dir = (player.position - origin).normalized;

        if (Physics.Raycast(origin, dir, out hit, viewDistance))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugDraw) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 forward = transform.forward;
        Vector3 leftLimit = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 rightLimit = Quaternion.Euler(0, viewAngle / 2, 0) * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightLimit * viewDistance);
    }
}
