using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Teacher : MonoBehaviour
{
    [Header("Behavior Type")]
    public bool isPatrolTeacher = false;
    public Transform[] patrolPoints;
    private int currentPointIndex = 0;

    [Header("View Settings")]
    public float viewAngle = 60f;          // 제자리 교사 시야각
    public float viewDistance = 5f;        // 감지 거리
    public float loseSightDistance = 7f;   // 추격 중 플레이어를 잃는 거리

    [Header("Combat Settings")]
    public float attackDamage = 50f;
    public float attackInterval = 1f;
    public float stopDistance = 1.2f;
    private float attackTimer = 0f;
    private bool hasAttackedOnce = false;

    [Header("Movement Settings")]
    public float moveSpeed = 1.1f;
    private NavMeshAgent agent;
    private Transform player;
    private PlayerController playerController;

    [Header("Rotation Settings (for stationary teacher)")]
    public float rotationInterval = 3f;
    private float rotationTimer = 0f;
    private bool isRotating = false;
    private Quaternion targetRotation;

    [Header("Boundary Settings")]
    public bool useLeash = true;
    public float leashRadius = 8f;

    [Tooltip("에이전트가 이동할 수 있는 NavMesh Area 마스크 (예: Indoor만).")]
    public int areaMask = ~0; // 기본: 전부 허용 (Navigation 창에서 Indoor만 체크 권장)

    [Header("Door Interaction")]
    public bool allowDoorInteraction = true;
    public LayerMask doorLayer = ~0; // 문이 있는 레이어만 체크하도록 설정 권장

    [Header("Animation")]
    public Animator animator;
    public float speedDampTime = 0.1f;

    [Header("Audio")]
    public AudioSource detectionAudio;
    public AudioSource attackAudio;


    // 내부 상태
    private bool hasPlayedDetectionSound = false;
    private bool isChasingPlayer = false;
    private Vector3 startPosition;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player?.GetComponent<PlayerController>();

        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.autoTraverseOffMeshLink = false; // 쓸데없는 링크(난간/외부) 타지 않게

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator)
            animator.applyRootMotion = false;

        // 교사의 시작 위치 저장 (복귀용)
        startPosition = transform.position;

        if (isPatrolTeacher)
        {
            GoToNextPatrolPoint();
        }
    }

    void Update()
    {
        UpdateAnimatorByMovement();

        if (isPatrolTeacher) HandlePatrolling();
        else HandleStationaryChase();
    }

    // 🎞️ 애니메이터 속도 갱신
    void UpdateAnimatorByMovement()
    {
        if (animator == null) return;
        float speed = (agent != null) ? agent.velocity.magnitude : 0f;
        animator.SetFloat("Speed", speed, speedDampTime, Time.deltaTime);
    }

    // 안전 목적지 설정: Leash + areaMask + NavMesh 샘플링
    bool TrySetDestination(Vector3 target)
    {
        Vector3 clamped = target;

        if (useLeash)
        {
            Vector3 to = target - startPosition;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist > leashRadius)
                clamped = startPosition + to.normalized * leashRadius;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(clamped, out hit, 1.0f, areaMask))
        {
            agent.isStopped = false;
            return agent.SetDestination(hit.position);
        }
        return false;
    }

    // 🧍 제자리 교사 + 추격/복귀 AI
    void HandleStationaryChase()
    {
        if (player == null || playerController == null || agent == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 감지 전: 절대 이동 금지
        if (!isChasingPlayer)
        {
            if (IsPlayerInFrontRange())
            {
                isChasingPlayer = true;
                if (detectionAudio && !detectionAudio.isPlaying) detectionAudio.Play();
            }
            else
            {
                // 제자리 회전만
                agent.isStopped = true;
                agent.ResetPath();
                RotateSmoothly();
                return;
            }
        }

        // 추격 상태
        if (isChasingPlayer)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            CheckForDoorAndInteract();

            // 공격 거리 도달 시
            if (distanceToPlayer <= stopDistance + 0.1f)
            {
                agent.isStopped = true;

                // 도달 즉시 첫 공격
                if (!hasAttackedOnce)
                {
                    hasAttackedOnce = true;
                    attackTimer = 0f;
                    playerController.TakeDamage(attackDamage);
                    if (attackAudio) attackAudio.Play();
                    Debug.Log($"{gameObject.name} performed first attack on player (stationary).");
                }

                // 이후 주기 공격
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackInterval)
                {
                    attackTimer = 0f;
                    playerController.TakeDamage(attackDamage);
                    if (attackAudio) attackAudio.Play();
                }
            }
            else
            {
                attackTimer = 0f;
                hasAttackedOnce = false;
            }

            // 플레이어가 너무 멀어지면 복귀
            if (distanceToPlayer > loseSightDistance)
            {
                isChasingPlayer = false;
                agent.SetDestination(startPosition);
                hasAttackedOnce = false;
                attackTimer = 0f;
            }
        }
        else
        {
            // 복귀 또는 대기 상태
            float distanceToStart = Vector3.Distance(transform.position, startPosition);

            if (distanceToStart > 0.5f)
            {
                agent.isStopped = false;
                agent.SetDestination(startPosition);
            }
            else
            {
                agent.isStopped = true;
                RotateSmoothly();
            }
        }
    }

    // 🌀 제자리 회전
    void RotateSmoothly()
    {
        rotationTimer += Time.deltaTime;
        if (!isRotating && rotationTimer >= rotationInterval)
        {
            rotationTimer = 0f;
            isRotating = true;
            targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180f, 0);
        }

        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 90f * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
                isRotating = false;
        }
    }

    // 🚶 순찰 교사
    void HandlePatrolling()
    {
        if (player == null || agent == null || playerController == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (IsPlayerInFrontRange())
        {
            if (!hasPlayedDetectionSound && detectionAudio != null)
            {
                detectionAudio.Play();
                hasPlayedDetectionSound = true;
            }

            // 플레이어 방향 회전
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(lookDir);

            // 공격 거리 도달 시
            if (distance <= stopDistance + 0.1f)
            {
                agent.isStopped = true;

                // 도착 즉시 첫 공격
                if (!hasAttackedOnce)
                {
                    hasAttackedOnce = true;
                    attackTimer = 0f;
                    playerController.TakeDamage(attackDamage);
                    if (attackAudio != null) attackAudio.Play();
                    Debug.Log($"{gameObject.name} performed first attack (patrolling).");
                }

                // 이후 주기 공격
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackInterval)
                {
                    attackTimer = 0f;
                    playerController.TakeDamage(attackDamage);
                    if (attackAudio != null) attackAudio.Play();
                }
            }
            else
            {
                // 아직 멀면 추격
                agent.isStopped = false;
                agent.SetDestination(player.position);
                CheckForDoorAndInteract();
                attackTimer = 0f;
                hasAttackedOnce = false;
            }
        }
        else
        {
            hasPlayedDetectionSound = false;

            // 순찰 포인트 도착 시 다음 포인트로 이동
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
                GoToNextPatrolPoint();
        }
    }

    // 👀 시야 판정: 제자리 교사는 각도+거리, 순찰 교사는 거리만
    bool IsPlayerInFrontRange()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = player.position - transform.position;
        dirToPlayer.y = 0f;

        float distance = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer.normalized);

        if (isPatrolTeacher)
            return distance <= viewDistance; // 순찰 교사는 거리만 체크
        else
            return (distance <= viewDistance && angle <= viewAngle / 2f);
    }

    // 🚪 문 열기 (Director 방식)
    private void CheckForDoorAndInteract()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 forward = transform.forward;
        float checkDistance = 2.0f;

        if (Physics.Raycast(origin, forward, out hit, checkDistance))
        {
            SuburbanHouse.Door door = hit.collider.GetComponent<SuburbanHouse.Door>();
            if (door != null)
            {
                door.OpenDoorForMonster();
                Debug.Log($"{gameObject.name} opened a door.");
            }
        }
    }

    // 🗺️ 순찰 포인트 순환
    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    public void SetAsPatrollingTeacher()
    {
        isPatrolTeacher = true;
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        GoToNextPatrolPoint();
        Debug.Log($"{gameObject.name} is now Patrol Teacher.");
    }

    // 🎨 디버그 시야 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, leftBoundary * viewDistance);
        Gizmos.DrawRay(transform.position, rightBoundary * viewDistance);
    }
}
