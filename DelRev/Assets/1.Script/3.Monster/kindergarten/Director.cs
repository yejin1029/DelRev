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
    private Animator animator;

    [Header("Guide (Greeting) Settings")]
    public Transform[] guidePoints;
    private int currentGuideIndex = 0;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Safety Zone Settings")]
    public Transform safetyExitPoint;

    [Header("Combat Settings")]
    public float detectionRange = 5f;
    public float attackRange = 2f;
    public float damageInterval = 1f;
    private float damageTimer = 0f;

    [Header("Speed Settings")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 2f;
    public float alertSpeed = 6f;

    [Header("Sound Settings (AudioSource 직접 지정)")]
    public AudioSource chaseSoundSource;   // 🔊 추격 사운드
    public AudioSource alertSoundSource;   // 🔊 경계 사운드
    public AudioSource attackSoundSource;  // 🔊 공격 사운드

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

        if (animator) animator.applyRootMotion = false;

        currentState = State.Greeting;
        StartCoroutine(GreetingRoutine());
    }

    void Update()
    {
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

    void UpdateAnimatorByAgent()
    {
        if (!animator || !agent) return;
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }

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
        GoToNextPatrolPoint();
    }

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

            if (chaseSoundSource && !chaseSoundSource.isPlaying)
                chaseSoundSource.Play();
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void ChaseUpdate(float distanceToPlayer)
    {
        if (AreaGaugeController.PlayerInSafetyZone && currentState != State.Alert)
        {
            if (safetyExitPoint != null)
            {
                currentState = State.Patrol;
                agent.speed = alertSpeed;
                agent.SetDestination(safetyExitPoint.position);
                currentPatrolIndex = 4;
                PatrolUpdate(distanceToPlayer);
            }
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
                playerController.health -= 40f;

                if (attackSoundSource)
                    attackSoundSource.Play();
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    void AlertUpdate(float distanceToPlayer)
    {
        CheckForDoorAndInteract();

        agent.speed = alertSpeed;
        agent.SetDestination(playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= 120f;

                if (attackSoundSource)
                    attackSoundSource.Play();
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    public void OnDangerGaugeMaxed()
    {
        currentState = State.Alert;
        agent.speed = alertSpeed;

        if (alertSoundSource && !alertSoundSource.isPlaying)
            alertSoundSource.Play();
    }

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
                door.OpenDoorForMonster();
        }
    }
}
