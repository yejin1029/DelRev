using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// Monster Follow route
public class Mom : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPatrolIndex;
    private enum State { Patrol, Chase, Return, Alert }
    private State currentState;
    private Vector3 lastPatrolPosition;
    private Transform playerTransform;
    private NavMeshAgent agent;

    public Transform introTriggerPoint;      // 이벤트 발생 지점
    public float introTriggerRadius = 0.5f;    // 플레이어가 근처에 오면 이벤트 발생
    public float introApproachDistance = 3f; // 몬스터가 어느 정도까지 다가가는지
    private bool hasDoneIntro = false;       // 한 번만 실행되게

    public float detectionRange = 3f;
    public float attackRange = 2f;
    public float damageAmount = 30f;     // 💥 1초당 줄 데미지
    public float damageInterval = 1f;    // ⏱️ 1초마다
    private float damageTimer = 0f;

    private PlayerController playerController;

    private bool isShiftPressed = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 초기 상태는 Patrol이 아닌 대기 상태로 설정
        currentState = State.Patrol;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        startintro(); // 플레이어가 introTriggerPoint 근처에 올 때까지 기다린 후 이벤트 실행
    }

    void startintro()
    {
        if (introTriggerPoint == null || hasDoneIntro) return;

        float distance = Vector3.Distance(playerTransform.position, introTriggerPoint.position);
        if (distance < introTriggerRadius)
        {
            hasDoneIntro = true; // 한번만 실행되도록 설정
            StartCoroutine(IntroApproachThenReturn());
        }
    }

    void Update()
    {
        if (playerTransform == null || playerController == null) return;
        // Ctrl 키가 눌렸는지 확인
        isShiftPressed = Input.GetKey(KeyCode.LeftControl);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case State.Patrol:
                if (isShiftPressed && distanceToPlayer > 2f)
                {
                    currentState = State.Patrol;
                }
                else if (distanceToPlayer < detectionRange && HasLineOfSight())
                {
                    currentState = State.Chase;
                }
                else if (!agent.pathPending && agent.remainingDistance < 5f)
                {
                    GoToNextPatrolPoint();
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
                if (distanceToPlayer > 2f && !HasLineOfSight()) // 플레이어가 충분히 멀어지면
                {
                    currentState = State.Patrol;
                    GoToNextPatrolPoint();
                }
                else if (distanceToPlayer < detectionRange && HasLineOfSight())
                {
                    currentState = State.Chase;
                }
                else if (!agent.pathPending && agent.remainingDistance < 5f)
                {
                    currentState = State.Patrol;
                    GoToNextPatrolPoint();
                }
                break;

            case State.Alert:
                // 위험 상태에선 무조건 플레이어 추격
                agent.SetDestination(playerTransform.position);
                break;
        }

        // 📌 공격 범위 안이면 데미지 주기
        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= damageAmount;
                Debug.Log($"💥 Monster attacked! Player HP: {playerController.health}");
            }
        }
        else
        {
            // 범위 벗어나면 타이머 초기화
            damageTimer = 0f;
        }
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
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 💥 공격 범위 시각화 (stopping distance)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, GetComponent<NavMeshAgent>().stoppingDistance);
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
    
    IEnumerator IntroApproachThenReturn()
    {
        currentState = State.Return; // 임시 상태로 이동 제어
        Vector3 originalPosition = transform.position;

        // 플레이어 근처로 이동
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        Vector3 targetPos = playerTransform.position + directionToPlayer * introApproachDistance;
        agent.SetDestination(targetPos);

        // 플레이어 근처로 이동할 때까지 대기 (2f 근처까지 다가가면 멈추기)
        while (Vector3.Distance(transform.position, targetPos) > 2f)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                break;
            }
            yield return null;
        }

        // 잠시 대기
        yield return new WaitForSeconds(1.5f);

        // 본래 위치로 복귀
        agent.SetDestination(originalPosition);

        // 원래 위치로 돌아갈 때까지 대기 (2f 이내로 돌아가면 멈추기)
        while (Vector3.Distance(transform.position, originalPosition) > 2f)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                break;
            }
            yield return null;
        }

        // Patrol로 상태 변경 및 순찰 지점으로 이동
        currentState = State.Patrol;
        GoToNextPatrolPoint();
    }
}
