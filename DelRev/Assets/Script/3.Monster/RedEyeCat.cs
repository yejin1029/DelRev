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

    public Transform centerPoint;  // 중심점으로 변경

    private float patrolTimer = 0f;
    private float stareTimer = 0f;
    private float damageTimer = 0f;
    private float damageInterval = 1f; // 데미지를 줄 간격
    private bool isStaring = false;

    private NavMeshAgent agent;
    private Transform player;
    private PlayerController playerController; // 플레이어의 상태를 참조

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>(); // 플레이어의 컨트롤러 가져오기

        GoToRandomPatrolPoint();
    }

    void Update()
    {
        if (player == null || playerController == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLOS = HasLineOfSight();

        switch (currentState)
        {
            case State.Patrol:
                HandlePatrol();
                if (hasLOS)
                {
                    stareTimer += Time.deltaTime;
                    if (stareTimer >= stareTimeThreshold)
                    {
                        currentState = State.Chase;
                        agent.speed = chaseSpeed;
                    }
                }
                else
                {
                    stareTimer = 0f;
                }
                break;

            case State.Chase:
                if (!hasLOS || distanceToPlayer > patrolRadius * 1.5f)
                {
                    currentState = State.Return;
                    agent.speed = 1.5f;
                    agent.SetDestination(centerPoint.position); // centerPoint로 돌아가기
                    stareTimer = 0f;
                }
                else
                {
                    agent.SetDestination(player.position);
                    LookAtPlayer();

                    if (distanceToPlayer <= agent.stoppingDistance + 0.5f)
                    {
                        // 📌 공격 범위 안이면 데미지 주기
                        damageTimer += Time.deltaTime;
                        if (damageTimer >= damageInterval)
                        {
                            damageTimer = 0f;
                            playerController.health -= attackDamage; // 플레이어의 체력 감소
                            Debug.Log($"💥 Red-eyed cat attacked! Player HP: {playerController.health}");
                        }
                    }
                    else
                    {
                        // 범위 벗어나면 타이머 초기화
                        damageTimer = 0f;
                    }
                }
                break;

            case State.Return:
                if (Vector3.Distance(transform.position, centerPoint.position) < 1.0f)
                {
                    currentState = State.Patrol;
                    GoToRandomPatrolPoint();
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
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += centerPoint.position;  // centerPoint를 기준으로 랜덤 위치 계산
        randomDirection.y = transform.position.y;

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

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer.normalized, out RaycastHit hit, viewDistance))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    void LookAtPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f; // 무빙이 아닌, y축 회전만 고려
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // 부드럽게 회전
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPoint == null ? transform.position : centerPoint.position, patrolRadius);  // centerPoint로 변경

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
