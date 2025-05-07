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
    public float eyeContactTime = 2f;
    public float chaseSpeed = 2.0f;
    public float attackDamage = 50f;
    public float attackSpeed = 1.5f;

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

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj?.transform;
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();

        if (player == null)
            Debug.LogError("❌ Player with 'Player' tag not found in the scene.");
        if (playerCamera == null)
            Debug.LogError("❌ Player's camera not found.");

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
                if (isWithinDetection && isLookingAtCat && hasLineOfSight)
                {
                    agent.isStopped = true;
                    transform.LookAt(player);
                    lookTimer += Time.deltaTime;
                    if (lookTimer >= eyeContactTime)
                    {
                        eyeContactTriggered = true;
                    }
                }
                else
                {
                    agent.isStopped = false;

                    if (eyeContactTriggered)
                    {
                        currentState = State.Aggressive;
                        agent.speed = chaseSpeed;
                    }
                    else
                    {
                        lookTimer = 0f;
                        if (!agent.pathPending && agent.remainingDistance < 1f)
                        {
                            waitTimer += Time.deltaTime;
                            if (waitTimer >= waitTime)
                            {
                                GoToRandomPosition();
                                waitTimer = 0f;
                            }
                        }
                    }
                }
                break;

            case State.Aggressive:
                agent.isStopped = false;

                if (attackCooldownTimer > 0)
                {
                    attackCooldownTimer -= Time.deltaTime;
                }

                agent.SetDestination(player.position);

                if (distanceToPlayer <= agent.stoppingDistance + 0.5f && attackCooldownTimer <= 0)
                {
                    AttackPlayer();
                }

                if (!isWithinDetection || !hasLineOfSight || !IsWithinPatrolRange())
                {
                    currentState = State.Return;
                    agent.speed = 1.0f;
                    GoToRandomPosition();
                }
                break;

            case State.Return:
                agent.isStopped = false;

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
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.health -= attackDamage;
            Debug.Log($"💥 Blue-eyed cat attacked! Player HP: {pc.health}");
            attackCooldownTimer = attackSpeed;
        }
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

        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Vector3 leftRay = Quaternion.Euler(0, -detectionAngle, 0) * playerCamera.transform.forward;
            Vector3 rightRay = Quaternion.Euler(0, detectionAngle, 0) * playerCamera.transform.forward;
            Gizmos.DrawRay(playerCamera.transform.position, leftRay * detectionDistance);
            Gizmos.DrawRay(playerCamera.transform.position, rightRay * detectionDistance);
        }
    }
}
