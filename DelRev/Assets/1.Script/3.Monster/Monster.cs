using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    private Transform playerTransform;
    private NavMeshAgent agent;

    public float detectionRange = 5f;
    public float damageAmount = 10f;     // 💥 1초당 줄 데미지
    public float damageInterval = 1f;    // ⏱️ 1초마다
    private float damageTimer = 0f;

    private PlayerController playerController;

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

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 플레이어가 감지 범위 안에 있고 시야에 보일 때만 추적
        if (distanceToPlayer <= detectionRange && HasLineOfSight())
        {
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
