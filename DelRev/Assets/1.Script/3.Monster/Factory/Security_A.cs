using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Security_A : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("순찰 경로 포인트들 (빈 오브젝트 배열)")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Speed Settings")]
    [Tooltip("순찰 시 속도")]
    public float patrolSpeed = 3.5f;
    [Tooltip("플레이어를 추격할 때 속도")]
    public float chaseSpeed = 5f;

    [Header("Combat Settings")]
    [Tooltip("몬스터가 플레이어를 인식하는 거리")]
    public float detectionRange = 5f;
    [Tooltip("몬스터가 플레이어를 공격하는 거리")]
    public float attackRange = 2f;
    [Tooltip("공격 간격 (초)")]
    public float damageInterval = 1f;
    [Tooltip("한 번 공격 시 입히는 피해량")]
    public float damageAmount = 20f;

    private float damageTimer = 0f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerController playerController;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        // 순찰 시작
        if (patrolPoints.Length > 0)
        {
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            // 플레이어 발견 → 추적
            agent.speed = chaseSpeed;
            agent.SetDestination(playerTransform.position);

            // 공격 범위 안에 들어왔을 때
            if (distanceToPlayer <= attackRange)
            {
                damageTimer += Time.deltaTime;
                if (damageTimer >= damageInterval)
                {
                    damageTimer = 0f;
                    if (playerController != null)
                    {
                        playerController.health -= damageAmount;
                        Debug.Log($"[Security_A] 플레이어 공격! 피해량: {damageAmount}");
                    }
                }
            }
            else
            {
                damageTimer = 0f; // 범위 벗어나면 공격 쿨 초기화
            }
        }
        else
        {
            // 플레이어가 detectionRange 밖으로 벗어나면 → 다시 순찰
            Patrol();
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }
}
