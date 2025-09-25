using UnityEngine;

public class CCTV : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 3f;
    public float detectionAngle = 120f;
    public float requiredStayTime = 3f;

    [Header("Target Guard")]
    public SecurityGuard targetGuard; // 👈 Inspector에서 연결

    private Transform playerTransform;
    private float detectionTimer = 0f;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null || targetGuard == null) return;

        if (IsPlayerInView())
        {
            detectionTimer += Time.deltaTime;

            if (detectionTimer >= requiredStayTime)
            {
                Debug.Log("[CCTV] 플레이어 3초 감지 → 경비원 호출");
                targetGuard.StartChase(playerTransform);
                detectionTimer = 0f;
            }
        }
        else
        {
            detectionTimer = 0f;
        }
    }

    private bool IsPlayerInView()
    {
        Vector3 dirToPlayer = playerTransform.position - transform.position;
        float distance = dirToPlayer.magnitude;

        if (distance > detectionRange) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > detectionAngle / 2f) return false;

        if (Physics.Raycast(transform.position, dirToPlayer.normalized, out RaycastHit hit, detectionRange))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }
}
