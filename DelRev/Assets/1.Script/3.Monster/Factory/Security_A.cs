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

    [Header("Audio Sources")]
    [Tooltip("플레이어를 발견했을 때 재생할 소리 (경보음 등)")]
    public AudioSource detectAudio;
    [Tooltip("공격 시 재생할 소리")]
    public AudioSource attackAudio;

    private bool isChasing = false; // 추격 중 여부 체크용

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
            // 플레이어 발견 → 추격 시작
            agent.speed = chaseSpeed;
            agent.SetDestination(playerTransform.position);

            // 🎧 추격 사운드 (최초 감지 시 한 번만 재생)
            if (!isChasing)
            {
                isChasing = true;
                if (detectAudio != null && !detectAudio.isPlaying)
                    detectAudio.Play();
            }

            // 공격 범위 안에 들어왔을 때
            if (distanceToPlayer <= attackRange)
            {
                damageTimer += Time.deltaTime;
                if (damageTimer >= damageInterval)
                {
                    damageTimer = 0f;
                    if (playerController != null)
                    {
                        playerController.TakeDamage(damageAmount); // ✅ 정식 대미지 처리

                        // 🎧 공격 사운드
                        if (attackAudio != null)
                            attackAudio.Play();

                        Debug.Log($"[Security_A] 플레이어 공격! 피해량: {damageAmount}");
                    }
                }
            }
            else
            {
                damageTimer = 0f; // 범위 벗어나면 쿨 초기화
            }
        }
        else
        {
            // 감지 범위 밖 → 순찰 모드 복귀
            if (isChasing)
                isChasing = false;

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
