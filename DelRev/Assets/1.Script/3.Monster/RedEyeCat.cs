using UnityEngine;
using UnityEngine.AI;

public class RedEyeCat : MonoBehaviour
{
    private enum State { Patrol, Aggressive }
    private State currentState = State.Patrol;

    public float patrolRadius = 3f;
    public float waitTime = 2f;
    public float detectionAngle = 30f;
    public float detectionDistance = 10f;
    public float eyeContactTime = 2f;
    public float chaseSpeed = 2.0f;
    public float attackDamage = 50f;
    public float attackSpeed = 1.5f;
    public float closeDetectionDistance = 2f;

    public Transform centerPoint;
    public Transform player;
    public Camera playerCamera;

    private float lookTimer;
    private float attackCooldownTimer = 0f;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        agent.stoppingDistance = 1.0f; 

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
                    transform.LookAt(player);
                    agent.isStopped = true;
                    lookTimer += Time.deltaTime;

                    if (lookTimer >= eyeContactTime)
                    {
                        currentState = State.Aggressive;
                        agent.speed = chaseSpeed;
                        agent.isStopped = false;
                    }
                }
                else
                {
                    lookTimer = 0f;
                    agent.isStopped = false;

                    if (!agent.pathPending && agent.remainingDistance < 1f)
                    {
                        GoToRandomPosition();
                    }
                }
                break;

            case State.Aggressive:
                if (attackCooldownTimer > 0)
                    attackCooldownTimer -= Time.deltaTime;

                agent.SetDestination(player.position);

                if (distanceToPlayer <= agent.stoppingDistance + 0.5f && attackCooldownTimer <= 0)
                {
                    AttackPlayer();
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
        float distanceToCat = Vector3.Distance(playerCamera.transform.position, transform.position);

        return (angle < detectionAngle && distanceToCat <= detectionDistance) || distanceToCat <= closeDetectionDistance;
    }

    void GoToRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + centerPoint.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
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
            attackCooldownTimer = attackSpeed;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (centerPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(centerPoint.position, patrolRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        if (playerCamera != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 leftRay = Quaternion.Euler(0, -detectionAngle, 0) * playerCamera.transform.forward;
            Vector3 rightRay = Quaternion.Euler(0, detectionAngle, 0) * playerCamera.transform.forward;
            Gizmos.DrawRay(playerCamera.transform.position, leftRay * detectionDistance);
            Gizmos.DrawRay(playerCamera.transform.position, rightRay * detectionDistance);
        }
    }
}