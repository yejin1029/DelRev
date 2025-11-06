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

    [Header("Audio Sources")]
    [Tooltip("추격 시작 시 재생할 오디오 소스")]
    public AudioSource chaseAudio;
    [Tooltip("공격 시 재생할 오디오 소스")]
    public AudioSource attackAudio;

    private float nextLogTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0f;
        agent.autoBraking = false;

        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("[FactoryManager] Player를 찾을 수 없습니다!");
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

        // 플레이어 감지 시 추격 전환
        if (distanceToPlayer < detectionRange)
        {
            currentState = State.Chase;

            // 🎧 추격 시작 사운드
            if (chaseAudio != null && !chaseAudio.isPlaying)
                chaseAudio.Play();

            Debug.Log("[FactoryManager] 플레이어 감지 → 추격 시작");
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

    // -------- Chase --------
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

                // ✅ 공식 대미지 처리
                playerController.TakeDamage(40f);

                // 🎧 공격 사운드
                if (attackAudio != null)
                    attackAudio.Play();

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

    // -------- Alert --------
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

                // ✅ 공식 대미지 처리
                playerController.TakeDamage(120f);

                // 🎧 공격 사운드
                if (attackAudio != null)
                    attackAudio.Play();

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

    // -------- 목적지 갱신 --------
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
