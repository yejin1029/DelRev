using UnityEngine;
using UnityEngine.AI;

public class Teacher : MonoBehaviour
{
    public bool isPatrolTeacher = false;
    public Transform[] patrolPoints; // 순찰용 포인트
    private int currentPointIndex;

    public float rotationInterval = 3f;
    private float rotationTimer = 0f;
    private bool facingForward = true;

    public float attackDamage = 50f;
    public float detectionDistance = 5f;
    public float attackInterval = 1f;
    private float attackTimer = 0f;

    public float moveSpeed = 1.1f;
    private NavMeshAgent agent;
    private Transform player;
    private PlayerController playerController;

    public Animator animator;
    public float speedDampTime = 0.1f; // 전환 부드럽게

    public GameObject forwardViewIndicator; // Optional 시각화용

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player?.GetComponent<PlayerController>();

        if (animator == null) animator = GetComponent<Animator>();
        if (animator) animator.applyRootMotion = false; // 에이전트가 이동 담당

        if (isPatrolTeacher)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;
            GoToNextPatrol();
        }
    }

    void Update()
    {
        // 매 프레임 애니메이터 갱신
        UpdateAnimatorByMovement();

        if (isPatrolTeacher)
        {
            HandlePatrolling();
            return;
        }

        RotateView();

        if (facingForward && IsPlayerInClassroom())
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                playerController?.TakeDamage(attackDamage);
                Debug.Log("교사가 공격했습니다.");
            }
        }
        else
        {
            attackTimer = 0f;
        }
    }


    // 이동/정지 상태를 애니메이터에 전달
    void UpdateAnimatorByMovement()
    {
        if (animator == null) return;

        float speed = 0f;

        if (isPatrolTeacher && agent != null)
        {
            // NavMeshAgent 실제 속도(m/s)
            speed = agent.velocity.magnitude;
        }
        else
        {
            // 비순찰 모드: 제자리에서 회전만 하므로 이동속도 0
            // (회전도 달리기 모션 쓰고 싶다면 여기서 회전 각속도로 대체 가능)
            speed = 0f;
        }

        animator.SetFloat("Speed", speed, speedDampTime, Time.deltaTime);
    }

    void RotateView()
    {
        rotationTimer += Time.deltaTime;
        if (rotationTimer >= rotationInterval)
        {
            rotationTimer = 0f;
            facingForward = !facingForward;
            transform.Rotate(0, 180f, 0); // 앞/뒤로 전환

            if (forwardViewIndicator != null)
                forwardViewIndicator.SetActive(facingForward);
        }
    }

    bool IsPlayerInClassroom()
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        return distance < 3f && player.GetComponent<Rigidbody>().velocity.magnitude > 0.1f;
    }

    void HandlePatrolling()
    {
        if (player == null || agent == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer <= detectionDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                GoToNextPatrol();
            }
        }

        if (distToPlayer <= 1.5f)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                playerController?.TakeDamage(attackDamage);
            }
        }
        else
        {
            attackTimer = 0f;
        }
    }

    void GoToNextPatrol()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    public void SetAsPatrollingTeacher()
    {
        isPatrolTeacher = true;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        GoToNextPatrol();
        Debug.Log($"{gameObject.name} is now Patrol Teacher.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}
