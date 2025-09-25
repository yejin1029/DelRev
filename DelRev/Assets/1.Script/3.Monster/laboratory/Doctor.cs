using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Doctor : MonoBehaviour, IDangerTarget
{
    public enum State { Patrol, Chase, Alert }
    private State currentState;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerController playerController;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Combat Settings")]
    public float detectionRange = 20f;   // 플레이어 탐지 거리
    public float attackRange = 2f;       // 공격 거리
    public float attackDamage = 150f;    // 박사 공격력 (즉사급)
    public float damageInterval = 1f;    // 공격 주기
    private float damageTimer = 0f;

    [Header("Speed Settings")]
    public float patrolSpeed = 1f;       // 순찰 속도
    public float chaseSpeed = 0.3f;      // 추격 속도 (기본)
    public float alertSpeed = 5f;        // 경계 모드 속도

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        currentState = State.Patrol;
        GoToNextPatrolPoint();
        Debug.Log("[Doctor] 초기 상태: Patrol");
    }

    void Update()
    {
        if (playerTransform == null || playerController == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case State.Patrol:
                PatrolUpdate(distanceToPlayer);
                break;

            case State.Chase:
                ChaseUpdate(distanceToPlayer);
                break;

            case State.Alert:
                AlertUpdate(distanceToPlayer);
                break;
        }
    }

    // -------- Patrol --------
    void PatrolUpdate(float distanceToPlayer)
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();

        if (distanceToPlayer < detectionRange)
        {
            currentState = State.Chase;
            Debug.Log("[Doctor] 플레이어 발견 → Chase 시작");
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        Debug.Log($"[Doctor] 순찰 포인트 이동: {patrolPoints[currentPatrolIndex].name}");
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // -------- Chase --------
    void ChaseUpdate(float distanceToPlayer)
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(playerTransform.position);
        Debug.Log("[Doctor] 플레이어 추격 중...");

        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= attackDamage;
                Debug.Log("[Doctor] 플레이어 공격! (즉사급 데미지 150)");
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    // -------- Alert --------
    void AlertUpdate(float distanceToPlayer)
    {
        agent.speed = alertSpeed;
        agent.SetDestination(playerTransform.position);
        Debug.Log("[Doctor] ALERT 모드: 무조건 추격!");

        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= attackDamage;
                Debug.Log("[Doctor] ALERT 공격! (즉사급 데미지 150)");
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    // -------- DangerGauge 연동 --------
    public void OnDangerGaugeMaxed()
    {
        currentState = State.Alert;
        agent.speed = alertSpeed;
        Debug.Log("[Doctor] DangerGauge 100 → ALERT 모드 전환!");
    }
}
