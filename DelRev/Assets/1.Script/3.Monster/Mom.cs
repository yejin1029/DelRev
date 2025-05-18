using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// Monster Follow route with chase & attack sounds and chase-sound cooldown
[RequireComponent(typeof(NavMeshAgent))]
public class Mom : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPatrolIndex;

    private enum State { Patrol, Chase, Return, Alert }
    private State currentState;
    private State previousState;                // 이전 프레임 상태 저장용

    private Vector3 lastPatrolPosition;
    private Transform playerTransform;
    private NavMeshAgent agent;

    [Header("Intro Settings")]
    public Transform introTriggerPoint;
    public float introTriggerRadius = 0.5f;
    public float introApproachDistance = 3f;
    private bool hasDoneIntro = false;

    [Header("Combat Settings")]
    public float detectionRange = 3f;
    public float attackRange = 2f;
    public float damageAmount = 30f;     // 1초당 데미지
    public float damageInterval = 1f;    // 1초마다
    private float damageTimer = 0f;
    private PlayerController playerController;

    [Header("Sound Sources")]
    [Tooltip("추격 시작 시 재생할 AudioSource")]
    public AudioSource chaseSource;
    [Tooltip("공격할 때마다 재생할 AudioSource")]
    public AudioSource attackSource;

    [Header("Chase Sound Cooldown")]
    [Tooltip("추격 사운드 재생 후 재생되지 않도록 대기할 시간(초)")]
    public float chaseSoundCooldown = 7f;
    private float chaseSoundTimer = 0f;

    private bool isShiftPressed = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = State.Patrol;
        previousState = currentState;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        TryStartIntro();
    }

    void Update()
    {
        if (playerTransform == null || playerController == null)
            return;

        // chase-sound 쿨다운 타이머 감소
        if (chaseSoundTimer > 0f)
            chaseSoundTimer -= Time.deltaTime;

        // 이전 상태 저장
        previousState = currentState;

        // Shift (Ctrl) 키 체크
        isShiftPressed = Input.GetKey(KeyCode.LeftControl);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 상태 머신
        switch (currentState)
        {
            case State.Patrol:
                if (!(isShiftPressed && distanceToPlayer > 2f))
                {
                    if (distanceToPlayer < detectionRange && HasLineOfSight())
                    {
                        currentState = State.Chase;
                    }
                    else if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    {
                        GoToNextPatrolPoint();
                    }
                }
                break;

            case State.Chase:
                if (distanceToPlayer > detectionRange || !HasLineOfSight())
                {
                    lastPatrolPosition = patrolPoints[currentPatrolIndex].position;
                    currentState = State.Return;
                    agent.SetDestination(lastPatrolPosition);
                }
                else
                {
                    agent.SetDestination(playerTransform.position);
                }
                break;

            case State.Return:
                if (distanceToPlayer > 2f && !HasLineOfSight())
                {
                    currentState = State.Patrol;
                    GoToNextPatrolPoint();
                }
                else if (distanceToPlayer < detectionRange && HasLineOfSight())
                {
                    currentState = State.Chase;
                }
                else if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    currentState = State.Patrol;
                    GoToNextPatrolPoint();
                }
                break;

            case State.Alert:
                agent.SetDestination(playerTransform.position);
                break;
        }

        // 상태 전이 감지 & 추격 시작 사운드 재생 (쿨다운 체크)
        if (currentState == State.Chase 
            && previousState != State.Chase 
            && chaseSoundTimer <= 0f)
        {
            chaseSource?.Play();
            chaseSoundTimer = chaseSoundCooldown;
        }

        // 공격 범위 내 데미지 + 공격 사운드
        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= damageAmount;
                attackSource?.Play();
                Debug.Log($"💥 Monster attacked! Player HP: {playerController.health}");
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    // 플레이어가 introTriggerPoint 근처에 오면 이벤트 시작
    void TryStartIntro()
    {
        if (introTriggerPoint == null || hasDoneIntro) return;

        float dist = Vector3.Distance(playerTransform.position, introTriggerPoint.position);
        if (dist < introTriggerRadius)
        {
            hasDoneIntro = true;
            StartCoroutine(IntroApproachThenReturn());
        }
    }

    IEnumerator IntroApproachThenReturn()
    {
        currentState = State.Return;
        Vector3 originalPos = transform.position;

        // 플레이어 근처로 이동
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        Vector3 targetPos = playerTransform.position + dir * introApproachDistance;
        agent.SetDestination(targetPos);

        while (Vector3.Distance(transform.position, targetPos) > 2f)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                break;
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        // 원래 위치로 복귀
        agent.SetDestination(originalPos);
        while (Vector3.Distance(transform.position, originalPos) > 2f)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                break;
            yield return null;
        }

        currentState = State.Patrol;
        GoToNextPatrolPoint();
    }

    public void OnDangerGaugeMaxed()
    {
        currentState = State.Alert;
        agent.speed = 5f;
        damageAmount = 150f;
        Debug.Log("⚠️ 위험 상태 진입! 속도 및 공격력 증가");
    }

    bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, dir, out hit, detectionRange))
            return hit.collider.CompareTag("Player");
        return false;
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
