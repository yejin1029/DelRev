using UnityEngine;
using UnityEngine.AI;

public class Dog : MonoBehaviour
{
    private enum State { Patrol, Chase, Return }
    private State currentState;

    public Transform centerPoint;
    public float patrolRadius = 10f;
    public float waitTime = 2f;

    public float detectionRange = 5f;
    public float damageAmount = 10f;
    public float damageInterval = 1f;

    private float waitTimer = 0f;
    private float damageTimer = 0f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerController playerController;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = State.Patrol;
        GoToRandomPosition();

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
                    waitTimer += Time.deltaTime;
                    if (waitTimer >= waitTime)
                    {
                        GoToRandomPosition();
                        waitTimer = 0f;
                    }
                }
                break;

            case State.Chase:
                if (distanceToPlayer > detectionRange || !HasLineOfSight())
                {
                    currentState = State.Return;
                    GoToRandomPosition();
                }
                else
                {
                    agent.SetDestination(playerTransform.position);
                }
                break;

            case State.Return:
                if (!agent.pathPending && agent.remainingDistance < 3f)
                {
                    currentState = State.Patrol;
                    GoToRandomPosition();
                }
                break;
        }

        // 데미지 처리
        if (distanceToPlayer <= agent.stoppingDistance)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= damageAmount;
                Debug.Log($"💥 Dog attacked! Player HP: {playerController.health}");
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    void GoToRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += centerPoint.position;
        randomDirection.y = transform.position.y; // Y축 고정

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
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
        if (centerPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(centerPoint.position, patrolRadius);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (agent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
        }
    }
}
