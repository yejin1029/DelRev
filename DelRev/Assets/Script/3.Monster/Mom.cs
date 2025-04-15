using UnityEngine;
using UnityEngine.AI;

//Monster Follow route
public class Mom : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPatrolIndex;
    private enum State { Patrol, Chase, Return }
    private State currentState;
    private Vector3 lastPatrolPosition;
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

        currentState = State.Patrol;
        GoToNextPatrolPoint();

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

        switch (currentState)
        {
            case State.Patrol:
                if (distanceToPlayer < detectionRange && HasLineOfSight())
                {
                    currentState = State.Chase;
                }
                else if (!agent.pathPending && agent.remainingDistance < 3f)
                {
                    GoToNextPatrolPoint();
                }
                break;

            case State.Chase:
                if (distanceToPlayer > detectionRange || !HasLineOfSight())
                {
                    lastPatrolPosition = patrolPoints[currentPatrolIndex].position;
                    currentState = State.Return;
                    agent.SetDestination(lastPatrolPosition);
                }
                else
                {
                    agent.SetDestination(playerTransform.position);
                }
                break;

            case State.Return:
                if (distanceToPlayer < detectionRange && HasLineOfSight())
                {
                    currentState = State.Chase;
                }
                if (!agent.pathPending && agent.remainingDistance < 3f)
                {
                    currentState = State.Patrol;
                    GoToNextPatrolPoint();
                }
                break;
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

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        Debug.Log($"currentPatrolIndex = {currentPatrolIndex}");
    }
}
