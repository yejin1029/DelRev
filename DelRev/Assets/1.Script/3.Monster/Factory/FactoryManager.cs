using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FactoryManager : MonoBehaviour, IDangerTarget
{
    public enum State { Patrol, Chase, Alert }
    private State currentState;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerController playerController;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Combat Settings")]
    [SerializeField] private float detectionRange = 6f;   // 플레이어 감지 거리
    [SerializeField] private float attackRange = 2f;      // 공격 사정거리
    [SerializeField] private float damageInterval = 1f;   // 공격 간격
    private float damageTimer = 0f;

    [Header("Speed Settings")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float alertSpeed = 7f;

    // 로그 주기 관리
    private float nextLogTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0f;
        agent.autoBraking = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        currentState = State.Patrol;
        Debug.Log("[FactoryManager] 초기 상태: Patrol");
        GoToNextPatrolPoint();
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

    // -------- Patrol (순찰) --------
    void PatrolUpdate(float distanceToPlayer)
    {
        agent.speed = patrolSpeed;
        CheckForDoorAndInteract();

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();

        if (distanceToPlayer < detectionRange)
        {
            currentState = State.Chase;
            Debug.Log("[FactoryManager] 플레이어 감지 → 무조건 추격 시작");
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        if (Time.time >= nextLogTime)
        {
            Debug.Log($"[FactoryManager] 순찰 포인트 이동: {patrolPoints[currentPatrolIndex].name}");
            nextLogTime = Time.time + 5f;
        }

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // -------- Chase (무조건 추격 모드) --------
    void ChaseUpdate(float distanceToPlayer)
    {
        agent.speed = chaseSpeed;
        UpdateDestination("[FactoryManager][Chase]");

        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= 40f;

                if (Time.time >= nextLogTime)
                {
                    Debug.Log("[FactoryManager] 공격! (데미지 40)");
                    nextLogTime = Time.time + 5f;
                }
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    // -------- Alert (게이지 Max → 강화된 추격) --------
    void AlertUpdate(float distanceToPlayer)
    {
        agent.speed = alertSpeed;
        UpdateDestination("[FactoryManager][ALERT]");

        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= 120f;

                if (Time.time >= nextLogTime)
                {
                    Debug.Log("[FactoryManager] ALERT 공격! (데미지 120)");
                    nextLogTime = Time.time + 5f;
                }
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
        Debug.Log("[FactoryManager] DangerGauge 100 → ALERT 모드 전환!");
    }

    // -------- 목적지 갱신 + 디버그 --------
    private void UpdateDestination(string prefix)
    {
        if (playerTransform == null) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(playerTransform.position, out hit, 50f, NavMesh.AllAreas))
        {
            bool pathFound = agent.SetDestination(hit.position);

            if (Time.time >= nextLogTime)
            {
                Debug.Log($"{prefix} 경로 설정: {pathFound}, 목적지 = {hit.position}");
                nextLogTime = Time.time + 5f;
            }
        }
        else
        {
            if (Time.time >= nextLogTime)
            {
                Debug.LogWarning($"{prefix} 플레이어 근처 NavMesh를 찾지 못함!");
                nextLogTime = Time.time + 5f;
            }
        }
    }

    // -------- Util --------
    bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, dir, out hit, detectionRange))
            return hit.collider.CompareTag("Player");
        return false;
    }

    private void CheckForDoorAndInteract()
    {
        RaycastHit hit;
        Vector3 forward = transform.forward;
        float checkDistance = 2.5f;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, forward, out hit, checkDistance))
        {
            SuburbanHouse.Door door = hit.collider.GetComponent<SuburbanHouse.Door>();
            if (door != null)
            {
                if (Time.time >= nextLogTime)
                {
                    Debug.Log("[FactoryManager] 문 발견 → 열기 시도");
                    nextLogTime = Time.time + 5f;
                }
                door.OpenDoorForMonster();
            }
        }
    }
}
