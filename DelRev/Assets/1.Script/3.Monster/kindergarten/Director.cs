using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Director : MonoBehaviour, IDangerTarget
{
    public enum State { Greeting, Patrol, Chase, Alert }
    private State currentState;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerController playerController;

    // Animator 참조
    private Animator animator;


    [Header("Guide (Greeting) Settings")]
    public Transform[] guidePoints;
    private int currentGuideIndex = 0;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Safety Zone Settings")]
    public Transform safetyExitPoint;   // point(4)로 지정

    [Header("Combat Settings")]
    public float detectionRange = 5f;
    public float attackRange = 2f;
    public float damageInterval = 1f;
    private float damageTimer = 0f;

    [Header("Speed Settings")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 2f;
    public float alertSpeed = 6f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        // 루트모션을 쓰지 않을 때(권장): NavMeshAgent가 이동을 담당
        if (animator) animator.applyRootMotion = false;

        currentState = State.Greeting;
        Debug.Log("[Director] 초기 상태: Greeting");
        StartCoroutine(GreetingRoutine());
    }

    void Update()
    {
        // 항상 현재 속도를 Animator에 전달 ('가만히/이동중' 전환의 핵심)
        UpdateAnimatorByAgent();

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

    
    // NavMeshAgent -> Animator
    void UpdateAnimatorByAgent()
    {
        if (!animator || !agent) return;

        float speed = agent.velocity.magnitude; // m/s
        // 튐 방지용 댐핑(부드럽게 전환)
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }

    // -------- Greeting (환영) --------
    IEnumerator GreetingRoutine()
    {
        agent.speed = patrolSpeed;

        for (int i = 0; i < guidePoints.Length; i++)
        {
            agent.SetDestination(guidePoints[i].position);

            while (Vector3.Distance(transform.position, guidePoints[i].position) > 1f)
            {
                CheckForDoorAndInteract();
                yield return null;
            }

            while (Vector3.Distance(playerTransform.position, guidePoints[i].position) > 1.5f)
            {
                yield return null;
            }
        }

        currentState = State.Patrol;
        Debug.Log("[Director] Greeting 끝 → Patrol 시작");
        GoToNextPatrolPoint();
    }

    // -------- Patrol (순찰) --------
    void PatrolUpdate(float distanceToPlayer)
    {
        agent.speed = patrolSpeed;
        CheckForDoorAndInteract();

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();

        if (!AreaGaugeController.PlayerInSafetyZone && distanceToPlayer < detectionRange && HasLineOfSight())
        {
            currentState = State.Chase;
            Debug.Log("[Director] 플레이어 발견 → Chase 시작");
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        Debug.Log($"[Director] 순찰 포인트 이동: {patrolPoints[currentPatrolIndex].name}");
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // -------- Chase (추격) --------
    void ChaseUpdate(float distanceToPlayer)
    {
        // 🔹 Player가 SafetyZone 안에 있으면 point(4)로 강제 이동 (Alert 아닐 때)
        if (AreaGaugeController.PlayerInSafetyZone && currentState != State.Alert)
        {
            if (safetyExitPoint != null)
            {
                Debug.Log("[Director] Player SafetyZone 감지 → point(4)로 강제 이동");
                currentState = State.Patrol;        // 상태를 Patrol로 전환
                agent.speed = alertSpeed;           // 빠르게 이동
                agent.SetDestination(safetyExitPoint.position);
                currentPatrolIndex = 4;             // 이후 순찰 이어가기

                // 🔹 즉시 Patrol 로직 실행
                PatrolUpdate(distanceToPlayer);
            }
            return; // 플레이어 추적 금지
        }

        // 🔹 SafetyZone 밖일 때만 추격
        agent.speed = chaseSpeed;
        agent.SetDestination(playerTransform.position);
        Debug.Log("[Director] 플레이어 추격 중...");

        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= 40f;
                Debug.Log("[Director] 플레이어 공격! (데미지 40)");
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    // -------- Alert (경계) --------
    void AlertUpdate(float distanceToPlayer)
    {
        CheckForDoorAndInteract();

        agent.speed = alertSpeed;
        agent.SetDestination(playerTransform.position);
        Debug.Log("[Director] ALERT 모드: SafetyZone 무시 추격");

        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= 120f;
                Debug.Log("[Director] ALERT 공격! (데미지 120)");
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    // -------- SafetyZone 트리거 (이제 전역 상태만 사용) --------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafetyZone"))
        {
            Debug.Log("[Director] SafetyZone 트리거 감지 (무시하고 전역 상태만 사용)");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafetyZone"))
        {
            Debug.Log("[Director] SafetyZone 트리거 이탈 (무시하고 전역 상태만 사용)");
        }
    }

    // -------- DangerGauge 연동 --------
    public void OnDangerGaugeMaxed()
    {
        currentState = State.Alert;
        agent.speed = alertSpeed;
        Debug.Log("[Director] DangerGauge 100 → ALERT 모드 전환!");
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
                Debug.Log("[Director] 문 발견 → 열기 시도");
                door.OpenDoorForMonster();
            }
        }
    }
}
