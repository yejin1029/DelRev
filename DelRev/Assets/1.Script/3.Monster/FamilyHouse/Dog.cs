using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Dog : MonoBehaviour
{
    private enum State { Patrol, Chase, Return, Called }
    private State currentState;

    [Header("Patrol Settings")]
    public List<BoxCollider> patrolAreas; // 여러 영역 설정 가능
    public float waitTime = 2f;

    [Header("Detection & Attack")]
    public float detectionRange = 5f;
    [Tooltip("플레이어에게 데미지를 줄 사거리")]
    public float attackRange = 4f;
    public float damageAmount = 10f;
    public float damageInterval = 1f;

    [Header("Audio")]
    [Tooltip("Hierarchy에 있는 Sound 오브젝트의 AudioSource를 할당")]
    public AudioSource attackSoundSource;
    private float damageTimer = 0f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Transform PetCCTV = null;
    private PlayerController playerController;

    [SerializeField] private float lostSightDelay = 2f;
    private float lostSightTimer = 0f;
    private bool playerInSight = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackRange;

        // attackSoundSource는 에디터에서 할당되어 있다고 가정
        if (attackSoundSource == null)
            Debug.LogWarning("⚠️ Attack Sound Source가 할당되지 않았습니다!");

        currentState = State.Patrol;
        GoToRandomPosition();

        agent.updateRotation = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        if (playerTransform == null || playerController == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        playerInSight = HasLineOfSight();

        // 빠른 회전 처리
        if (currentState == State.Chase || currentState == State.Patrol || distanceToPlayer <= attackRange)
        {
            Vector3 direction;

            if (currentState == State.Chase && playerTransform != null)
                direction = (playerTransform.position - transform.position).normalized;
            else
                direction = agent.velocity.normalized;

            direction.y = 0;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }

        switch (currentState)
        {
            case State.Patrol:
                if (distanceToPlayer < detectionRange && playerInSight)
                {
                    currentState = State.Chase;
                }
                else if (!agent.pathPending && agent.remainingDistance < 1f)
                {
                    GoToRandomPosition();
                }
                break;

            case State.Chase:

                if (playerInSight)
                {
                    lostSightTimer = 0f;
                    agent.SetDestination(playerTransform.position);
                }
                else
                {
                    lostSightTimer += Time.deltaTime;
                    if (lostSightTimer >= lostSightDelay)
                    {
                        currentState = State.Return;
                        GoToRandomPosition();
                    }
                }
                break;

            case State.Return:
                if (distanceToPlayer < detectionRange && playerInSight)
                {
                    currentState = State.Chase;
                }
                else if (!agent.pathPending && agent.remainingDistance < 0.1f)
                {
                    currentState = State.Patrol;
                    GoToRandomPosition();
                }
                break;

            case State.Called:
                if (!agent.pathPending && agent.remainingDistance < 2f)
                {
                    GoToRandomPosition();
                    currentState = State.Patrol;
                }
                break;
        }

        // 공격 로직
        if (distanceToPlayer <= agent.stoppingDistance)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= damageAmount;

                // Hierarchy에 있는 AudioSource로 소리 재생
                if (attackSoundSource != null)
                    attackSoundSource.PlayOneShot(attackSoundSource.clip);
            }
        }
        else
        {
            damageTimer = 0f;
        }

    }

    public void MoveToCCTV(Vector3 cctvPosition)
    {
        Debug.Log("MoveToCCTV called with position: " + cctvPosition);

        PetCCTV = GameObject.FindGameObjectWithTag("PetCCTV").transform;
        PetCCTV.position = cctvPosition;

        agent.SetDestination(PetCCTV.position);
        RotateTowards(PetCCTV.position); // ← 회전 추가
        currentState = State.Called;
    }
    void GoToRandomPosition()
    {
        if (patrolAreas == null || patrolAreas.Count == 0) return;

        for (int attempts = 0; attempts < 10; attempts++)
        {
            BoxCollider selectedArea = patrolAreas[Random.Range(0, patrolAreas.Count)];
            Bounds bounds = selectedArea.bounds;

            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                transform.position.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);

                // 회전 방향 지정
                RotateTowards(hit.position);

                return;
            }
        }
    }

    bool HasLineOfSight()
    {
        Vector3 direction = playerTransform.position - transform.position;
        float distance = direction.magnitude;

        RaycastHit hit;
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
        {
            return hit.collider.CompareTag("Player");
        }

        if (distance < 1.5f) return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (patrolAreas != null)
        {
            foreach (var area in patrolAreas)
            {
                if (area != null)
                    Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (agent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
    
    void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }
    }
}
