using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SecurityGuard : MonoBehaviour
{
    public enum State { Patrol, CCTV, Chase }
    private State currentState;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerController playerController;

    [Header("Guard Settings")]
    public int guardID = 0;   // 0번(A) / 1번(B)
    public float patrolSpeed = 1.3f;
    public float attackRange = 2f;
    public float attackDamage = 40f;
    public float damageInterval = 1f;
    private float damageTimer = 0f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("CCTV Position")]
    public Transform cctvPosition;

    [Header("Chase Settings")]
    public float chaseDuration = 10f;
    private float chaseTimer = 0f;

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
    }

    void Update()
    {
        if (playerTransform == null || playerController == null) return;

        UpdateShiftByClock();

        switch (currentState)
        {
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.CCTV:
                CCTVUpdate();
                break;
            case State.Chase:
                ChaseUpdate();
                break;
        }
    }

    // ✅ 외부에서 호출하는 함수
    public void StartChase(Transform target)
    {
        if (currentState != State.Patrol) return; // 순찰 중일 때만 반응
        playerTransform = target;
        currentState = State.Chase;
        chaseTimer = 0f;
        Debug.Log($"[SecurityGuard {guardID}] 플레이어 추격 시작!");
    }

    void UpdateShiftByClock()
    {
        Clock clock = FindObjectOfType<Clock>();
        if (clock == null) return;

        int totalMinutes = clock.hour * 60 + clock.minutes;
        int halfHour = totalMinutes % 60;

        if (currentState == State.Chase) return;

        if (guardID == 0)
            currentState = (halfHour < 30) ? State.Patrol : State.CCTV;
        else
            currentState = (halfHour < 30) ? State.CCTV : State.Patrol;

        if (currentState == State.Patrol)
            GoToNextPatrolPoint();
        else if (currentState == State.CCTV && cctvPosition != null)
            agent.SetDestination(cctvPosition.position);
    }

    void PatrolUpdate()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void CCTVUpdate()
    {
        agent.ResetPath();
    }

    void ChaseUpdate()
    {
        chaseTimer += Time.deltaTime;

        if (chaseTimer >= chaseDuration)
        {
            Debug.Log($"[SecurityGuard {guardID}] 추격 종료 → 교대 상태로 복귀");
            UpdateShiftByClock();
            return;
        }

        agent.speed = patrolSpeed;
        agent.SetDestination(playerTransform.position);

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= attackDamage;
                Debug.Log($"[SecurityGuard {guardID}] 플레이어 공격! (-{attackDamage})");
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }
}
