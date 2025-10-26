using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DollMonsterAI : MonoBehaviour
{
    public enum State { Patrol, Chase }
    private State currentState;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerController playerController;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Combat Settings")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float damageInterval = 1f;
    public int attackDamage = 20;
    private float damageTimer = 0f;

    [Header("Speed Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3f;

    [Header("Animation")]
    public Animator animator;
    public float speedDampTime = 0.1f;

    [Header("Sound Settings (AudioSource 직접 지정)")]
    public AudioSource detectSoundSource;  // 🔊 쳐다봤을 때(발견 시)
    public AudioSource attackSoundSource;  // 🔊 공격할 때

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        if (animator == null) animator = GetComponent<Animator>();
        if (animator) animator.applyRootMotion = false;

        currentState = State.Patrol;
        if (patrolPoints.Length > 0)
            GoToNextPatrolPoint();
    }

    void Update()
    {
        if (playerTransform == null || playerController == null) return;

        UpdateAnimatorByAgent();

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool isPlayerCrouching = Input.GetKey(playerController.crouchKey);

        switch (currentState)
        {
            case State.Patrol:
                PatrolUpdate(distanceToPlayer, isPlayerCrouching);
                break;
            case State.Chase:
                ChaseUpdate(distanceToPlayer, isPlayerCrouching);
                break;
        }
    }

    void UpdateAnimatorByAgent()
    {
        if (animator == null || agent == null) return;
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed, speedDampTime, Time.deltaTime);
    }

    void PatrolUpdate(float distanceToPlayer, bool isPlayerCrouching)
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f && patrolPoints.Length > 0)
            GoToNextPatrolPoint();

        if (distanceToPlayer < detectionRange && !isPlayerCrouching)
        {
            currentState = State.Chase;
            Debug.Log("[DollMonster] 플레이어 발견 → Chase 시작");

            // 🔊 발견 사운드
            if (detectSoundSource && !detectSoundSource.isPlaying)
                detectSoundSource.Play();
        }

        CheckForDoorAndInteract();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void ChaseUpdate(float distanceToPlayer, bool isPlayerCrouching)
    {
        if (isPlayerCrouching || distanceToPlayer > detectionRange)
        {
            currentState = State.Patrol;
            Debug.Log("[DollMonster] 플레이어 놓침 → Patrol 복귀");
            if (patrolPoints.Length > 0)
                GoToNextPatrolPoint();
            return;
        }

        agent.speed = chaseSpeed;
        agent.SetDestination(playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                AttackPlayer();
            }
        }
        else
        {
            damageTimer = 0f;
        }

        CheckForDoorAndInteract();
    }

    void AttackPlayer()
    {
        Debug.Log("[DollMonster] 플레이어 공격!");

        if (playerController != null)
        {
            playerController.TakeDamage(attackDamage);
            Debug.Log($"[DollMonster] 플레이어 {attackDamage} 피해 입음. 남은 체력: {playerController.health}");

            // 🔊 공격 사운드
            if (attackSoundSource)
                attackSoundSource.Play();
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
                Debug.Log("[DollMonster] 문 발견 → 열기 시도");
                door.OpenDoorForMonster();
            }
        }
    }
}
