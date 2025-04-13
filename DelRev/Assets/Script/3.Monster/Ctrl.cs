using UnityEngine;
using UnityEngine.AI;

public class Ctrl : MonoBehaviour
{
    private Transform playerTransform;
    private NavMeshAgent agent;

    public float detectionRange = 5f;
    public float damageAmount = 10f;     // 💥 1초당 줄 데미지
    public float damageInterval = 1f;    // ⏱️ 1초마다
    private float damageTimer = 0f;

    private PlayerController playerController;

    private bool isShiftPressed = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        if (playerTransform == null || playerController == null) return;

        // Ctrl 키가 눌렸는지 확인
        isShiftPressed = Input.GetKey(KeyCode.LeftControl);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Ctrl가 눌리고, 플레이어가 너무 가까이 있지 않으면 감지하지 않음
        if (isShiftPressed && distanceToPlayer > 2f)
        {
            // Ctrl를 누르고 플레이어가 2f 이상 멀리 있으면 추적을 멈춤
            agent.ResetPath();
        }
        else if (distanceToPlayer <= detectionRange && HasLineOfSight())
        {
            // Ctrl가 안 눌렸거나, Ctrl 키가 눌려도 범위 안에 있으면 추적
            agent.SetDestination(playerTransform.position);
        }
        else
        {
            agent.ResetPath();
        }

        // 📌 공격 범위 안이면 데미지 주기
        if (distanceToPlayer <= agent.stoppingDistance)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= damageAmount;
                Debug.Log($"💥 Monster attacked! Player HP: {playerController.health}");
            }
        }
        else
        {
            // 범위 벗어나면 타이머 초기화
            damageTimer = 0f;
        }
    }

    bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 💥 공격 범위 시각화 (stopping distance)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, GetComponent<NavMeshAgent>().stoppingDistance);
    }
}