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
    public float attackRange = 2f;
    public float damageAmount = 10f;
    public float damageInterval = 1f;

    [Header("Audio")]
    [Tooltip("Hierarchy에 있는 Sound 오브젝트의 AudioSource를 할당")]
    public AudioSource attackSoundSource;

    private float waitTimer = 0f;
    private float damageTimer = 0f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Transform PetCCTV = null;
    private PlayerController playerController;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackRange;

        // attackSoundSource는 에디터에서 할당되어 있다고 가정
        if (attackSoundSource == null)
            Debug.LogWarning("⚠️ Attack Sound Source가 할당되지 않았습니다!");

        currentState = State.Patrol;
        GoToRandomPosition();

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

        switch (currentState)
        {
            case State.Patrol:
                if (distanceToPlayer < detectionRange && HasLineOfSight())
                {
                    currentState = State.Chase;
                }
                else if (!agent.pathPending && agent.remainingDistance < 0.1f)
                {
                    waitTimer += Time.deltaTime;
                    if (waitTimer >= waitTime)
                    {
                        GoToRandomPosition();
                        waitTimer = 0f;
                    }
                }
                break;

            case State.Chase:
                if (distanceToPlayer > detectionRange || !HasLineOfSight())
                {
                    currentState = State.Return;
                    GoToRandomPosition();
                }
                else
                {
                    agent.SetDestination(playerTransform.position);
                }
                break;

            case State.Return:
                if (distanceToPlayer < detectionRange && HasLineOfSight())
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
                if (!agent.pathPending && agent.remainingDistance < 0.1f)
                {
                    GoToRandomPosition();
                    currentState = State.Patrol;

                    if (PetCCTV != null)
                    {
                        Destroy(PetCCTV.gameObject);
                        PetCCTV = null;
                    }
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

                Debug.Log($"💥 Dog attacked! Player HP: {playerController.health}");
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    public void MoveToCCTV(Vector3 cctvPosition)
    {
        if (PetCCTV != null)
        {
            Destroy(PetCCTV.gameObject);
        }

        PetCCTV = GameObject.FindGameObjectWithTag("PetCCTV").transform;
        PetCCTV.position = cctvPosition;
        agent.SetDestination(PetCCTV.position);
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
                return;
            }
        }

        Debug.LogWarning("🐶 Dog couldn't find a valid random point on NavMesh!");
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
}
