using UnityEngine;
using UnityEngine.AI;

public class BlueEyeCat : MonoBehaviour
{
    private enum State { Patrol, Aggressive, Return }
    private State currentState = State.Patrol;

    public float patrolRadius = 5f;
    public float waitTime = 2f;
    public float detectionAngle = 30f;
    public float detectionDistance = 10f;
    public float eyeContactTime = 2f; // 눈 마주침 시간
    public float chaseSpeed = 2.0f; // 공격적인 상태에서의 이동 속도
    public float attackDamage = 50f; // 공격력
    public float attackSpeed = 1.5f; // 공격 속도 (쿨타임)

    public Transform centerPoint;
    public Transform player;
    public Camera playerCamera;

    private float waitTimer;
    private float lookTimer;
    private bool eyeContactTriggered;
    private float attackCooldownTimer = 0f;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = Camera.main; // 플레이어 카메라를 찾습니다.
        if (playerCamera == null)
        {
            Debug.LogError("Player camera not found. Please assign the player camera in the inspector.");
        }
        GoToRandomPosition();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLineOfSight = HasLineOfSight();
        bool isLookingAtCat = IsPlayerLookingAtCat();
        bool isWithinDetection = distanceToPlayer <= detectionDistance;

        switch (currentState)
        {
            case State.Patrol:
                if (!agent.pathPending && agent.remainingDistance < 1f)
                {
                    waitTimer += Time.deltaTime;
                    if (waitTimer >= waitTime)
                    {
                        GoToRandomPosition();
                        waitTimer = 0f;
                    }
                }

                if (isWithinDetection && isLookingAtCat && hasLineOfSight)
                {
                    lookTimer += Time.deltaTime;
                    if (lookTimer >= eyeContactTime)
                    {
                        eyeContactTriggered = true;
                    }
                }
                else
                {
                    if (eyeContactTriggered)
                    {
                        currentState = State.Aggressive;
                        agent.speed = chaseSpeed;
                    }
                    else
                    {
                        lookTimer = 0f;
                    }
                }
                break;

            case State.Aggressive:
                if (attackCooldownTimer > 0)
                {
                    attackCooldownTimer -= Time.deltaTime; // 공격 쿨타임
                }

                agent.SetDestination(player.position);
                if (distanceToPlayer <= agent.stoppingDistance + 0.5f)
                {
                    if (attackCooldownTimer <= 0)
                    {
                        AttackPlayer();
                    }
                }

                if (!isWithinDetection || !hasLineOfSight || !IsWithinPatrolRange())
                {
                    currentState = State.Return;
                    agent.speed = 1.0f;
                    GoToRandomPosition();
                }
                break;

            case State.Return:
                if (!agent.pathPending && agent.remainingDistance < 1f)
                {
                    currentState = State.Patrol;
                    eyeContactTriggered = false;
                    lookTimer = 0f;
                    GoToRandomPosition();
                }
                break;
        }
    }

    bool HasLineOfSight()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, detectionDistance))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    bool IsPlayerLookingAtCat()
    {
        Vector3 directionToCat = (transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToCat);
        return angle < detectionAngle;
    }

    bool IsWithinPatrolRange()
    {
        return Vector3.Distance(transform.position, centerPoint.position) <= patrolRadius;
    }

    void GoToRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + centerPoint.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void AttackPlayer()
    {
        // 공격 처리 로직
        Debug.Log($"💥 Blue-eyed cat attacked! Player HP: {player.GetComponent<PlayerController>().health - attackDamage}");
        player.GetComponent<PlayerController>().health -= attackDamage; // 플레이어 체력 감소
        attackCooldownTimer = attackSpeed; // 공격 후 쿨타임 적용
    }

    void OnDrawGizmosSelected()
    {
        if (centerPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(centerPoint.position, patrolRadius);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        Gizmos.color = Color.green;
        Vector3 leftRay = Quaternion.Euler(0, -detectionAngle, 0) * playerCamera.transform.forward;
        Vector3 rightRay = Quaternion.Euler(0, detectionAngle, 0) * playerCamera.transform.forward;
        Gizmos.DrawRay(playerCamera.transform.position, leftRay * detectionDistance);
        Gizmos.DrawRay(playerCamera.transform.position, rightRay * detectionDistance);
    }
}
