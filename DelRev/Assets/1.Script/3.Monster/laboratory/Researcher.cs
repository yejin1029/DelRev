using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Researcher : MonoBehaviour
{
    public enum State { Idle, Chase, Return }
    private State currentState;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerController playerController;

    [Header("Work Settings")]
    public Transform workPosition;
    private Vector3 initialPosition;

    [Header("Detection Settings")]
    public float detectionRange = 3f;
    public float loseSightRange = 6f;
    public float chaseSpeed = 0.7f;

    [Header("Combat Settings")]
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float damageInterval = 1f;
    private float damageTimer = 0f;

    [Header("Target Guard")]
    public SecurityGuard targetGuard; // 👈 Inspector에서 연결

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        initialPosition = (workPosition != null) ? workPosition.position : transform.position;
        currentState = State.Idle;
    }

    void Update()
    {
        if (playerTransform == null || playerController == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case State.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    Debug.Log("[Researcher] 플레이어 발견 → 경비원 호출 + 추격");
                    if (targetGuard != null)
                        targetGuard.StartChase(playerTransform);
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                agent.SetDestination(playerTransform.position);

                if (distanceToPlayer <= attackRange)
                {
                    damageTimer += Time.deltaTime;
                    if (damageTimer >= damageInterval)
                    {
                        damageTimer = 0f;
                        playerController.health -= attackDamage;
                        Debug.Log("[Researcher] 공격! (-10)");
                    }
                }
                else damageTimer = 0f;

                if (distanceToPlayer > loseSightRange)
                {
                    currentState = State.Return;
                    agent.SetDestination(initialPosition);
                }
                break;

            case State.Return:
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    currentState = State.Idle;
                }
                else if (distanceToPlayer <= detectionRange)
                {
                    if (targetGuard != null)
                        targetGuard.StartChase(playerTransform);
                    currentState = State.Chase;
                }
                break;
        }
    }
}
