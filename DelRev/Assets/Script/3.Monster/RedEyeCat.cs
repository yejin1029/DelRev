using UnityEngine;
using UnityEngine.AI;

public class RedEyeCat : MonoBehaviour
{
    private enum State { Patrol, Chase, Return }
    private State currentState = State.Patrol;

    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    public float viewDistance = 5f;
    public float viewAngle = 120f;
    public float stareTimeThreshold = 2f;
    public float chaseSpeed = 2.0f;
    public float attackDamage = 50f;

    public Transform centerPoint;

    private float patrolTimer = 0f;
    private float stareTimer = 0f;
    private float damageTimer = 0f;
    private float damageInterval = 1f;

    private NavMeshAgent agent;
    private Transform player;
    private PlayerController playerController;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player?.GetComponent<PlayerController>();

        if (player == null || playerController == null)
        {
            Debug.LogError("❌ 플레이어 또는 PlayerController가 설정되지 않았습니다.");
            enabled = false;
            return;
        }

        GoToRandomPatrolPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLOS = HasLineOfSight();

        switch (currentState)
        {
            case State.Patrol:
                if (hasLOS)
                {
                    agent.ResetPath();  // 즉시 정지
                    agent.isStopped = true;
                    LookAtPlayer();
                    stareTimer += Time.deltaTime;

                    if (stareTimer >= stareTimeThreshold)
                    {
                        currentState = State.Chase;
                        agent.isStopped = false;
                        agent.speed = chaseSpeed;
                        agent.SetDestination(player.position);  // 즉시 추격
                        Debug.Log("🐾 RedEyeCat: 추격 시작!");
                    }
                }
                else
                {
                    agent.isStopped = false;
                    stareTimer = 0f;
                    HandlePatrol();
                }
                break;

            case State.Chase:
                if (!hasLOS || distanceToPlayer > patrolRadius * 1.5f)
                {
                    currentState = State.Return;
                    agent.speed = 1.5f;
                    agent.SetDestination(centerPoint.position);
                    stareTimer = 0f;
                    Debug.Log("↩️ RedEyeCat: 돌아가는 중...");
                }
                else
                {
                    agent.SetDestination(player.position);
                    LookAtPlayer();

                    if (distanceToPlayer <= agent.stoppingDistance + 0.5f)
                    {
                        damageTimer += Time.deltaTime;
                        if (damageTimer >= damageInterval)
                        {
                            damageTimer = 0f;
                            playerController.health -= attackDamage;
                            Debug.Log($"💥 RedEyeCat 공격! Player HP: {playerController.health}");
                        }
                    }
                    else
                    {
                        damageTimer = 0f;
                    }
                }
                break;

            case State.Return:
                if (Vector3.Distance(transform.position, centerPoint.position) < 1.0f)
                {
                    currentState = State.Patrol;
                    GoToRandomPatrolPoint();
                    Debug.Log("🚶 RedEyeCat: 순찰로 복귀");
                }
                break;
        }
    }

    void HandlePatrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                GoToRandomPatrolPoint();
                patrolTimer = 0f;
            }
        }
    }

    void GoToRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + centerPoint.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    bool HasLineOfSight()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);
        if (angle > viewAngle / 2f) return false;

        Vector3 rayOrigin = transform.position + Vector3.up * 1.2f; // 머리 높이로 보정
        if (Physics.Raycast(rayOrigin, directionToPlayer.normalized, out RaycastHit hit, viewDistance))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    void LookAtPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPoint == null ? transform.position : centerPoint.position, patrolRadius);

        Gizmos.color = Color.red;
        Vector3 forward = transform.forward;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle / 2, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * forward * viewDistance;
        Vector3 rightRayDirection = rightRayRotation * forward * viewDistance;
        Gizmos.DrawRay(transform.position, leftRayDirection);
        Gizmos.DrawRay(transform.position, rightRayDirection);
    }
}
