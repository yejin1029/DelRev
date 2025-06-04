using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Mom : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPatrolIndex;

    public GameObject[] introTexts;

    private enum State { None, Patrol, Chase, Return, Alert }
    private State currentState;
    private State previousState;

    private Vector3 lastPatrolPosition;
    private Transform playerTransform;
    private NavMeshAgent agent;

    [Header("Intro Settings")]
    public Transform[] approachPoints;
    private float waitDistance = 1f;
    private float checkInterval = 0.5f;

    public Transform lastPatrolPoint = null;

    [Header("Combat Settings")]
    public float detectionRange = 3f;
    public float attackRange = 2f;
    public float damageAmount = 30f;
    public float damageInterval = 1f;
    private float damageTimer = 0f;
    private PlayerController playerController;

    [Header("Sound Sources")]
    public AudioSource chaseSource;
    public AudioSource attackSource;

    [Header("Chase Sound Cooldown")]
    public float chaseSoundCooldown = 7f;
    private float chaseSoundTimer = 0f;

    [Header("Intro Text Sounds")]
    [Tooltip("인트로 텍스트마다 재생할 사운드들 (Text1, Text2, Text3 순)")]
    public AudioSource[] introTextSounds;

    [Header("Rotation")]
    public float rotationSpeed = 10f; // 빠르게 회전
    private bool isShiftPressed = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = State.None;
        previousState = currentState;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        GameObject[] allTexts = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allTexts)
        {
            if (obj.name == "Mom_Text1") introTexts[0] = obj;
            if (obj.name == "Mom_Text2") introTexts[1] = obj;
            if (obj.name == "Mom_Text3") introTexts[2] = obj;
        }

        foreach (var t in introTexts)
        {
            if (t != null)
                t.SetActive(false);
            else
                Debug.LogWarning("Intro text not found. Check GameObject names (Text1, Text2, Text3).");
        }

        StartCoroutine(IntroApproachThenReturn());
    }

    void Update()
    {
        if (currentState == State.None) return;
        if (playerTransform == null || playerController == null) return;

        // 빠른 회전 처리
        RotateTowardsMovementDirection();

        if (chaseSoundTimer > 0f)
            chaseSoundTimer -= Time.deltaTime;

        previousState = currentState;
        isShiftPressed = Input.GetKey(KeyCode.LeftControl);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool hasSight = HasLineOfSight(); // 호출 1회로 줄이기

        switch (currentState)
        {
            case State.Patrol:
                if (!(isShiftPressed && distanceToPlayer > 2f))
                {
                    if (distanceToPlayer < detectionRange && hasSight)
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
                if (!hasSight)
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
                if (!hasSight)
                {
                    currentState = State.Patrol;
                    GoToNextPatrolPoint();
                }
                else if (hasSight)
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

        // 추격 사운드 처리
        if (currentState == State.Chase && previousState != State.Chase && chaseSoundTimer <= 0f)
        {
            chaseSource?.Play();
            chaseSoundTimer = chaseSoundCooldown;
        }

        // 공격 처리
        if (distanceToPlayer <= attackRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerController.health -= damageAmount;
                attackSource?.Play();
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
        agent.speed = 12f;
        damageAmount = 150f;
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

    private void CheckForDoorAndInteract()
    {
        RaycastHit hit;
        Vector3 forward = transform.forward;
        float checkDistance = 3.0f;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, forward, out hit, checkDistance))
        {
            SuburbanHouse.Door door = hit.collider.GetComponent<SuburbanHouse.Door>();
            if (door != null)
            {
                door.OpenDoorForMonster();
            }
        }
    }

    IEnumerator IntroApproachThenReturn()
    {
        for (int i = 0; i < approachPoints.Length; i++)
        {
            Vector3 point = approachPoints[i].position;
            agent.SetDestination(point);

            while (Vector3.Distance(transform.position, point) > 1f)
            {
                CheckForDoorAndInteract();
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    break;
                yield return null;
            }

            while (Vector3.Distance(playerTransform.position, point) > waitDistance)
            {
                yield return new WaitForSeconds(checkInterval);
            }

            SetIntroText(i);
            lastPatrolPoint = approachPoints[i];
        }

        if (lastPatrolPoint != null)
        {
            Vector3 point = lastPatrolPoint.position;
            agent.SetDestination(point);
            yield return new WaitForSeconds(2f);
            SetIntroText(-1);
        }

        yield return new WaitForSeconds(1f);
        currentState = State.Patrol;
        GoToNextPatrolPoint();
    }

    void SetIntroText(int index)
    {
        for (int i = 0; i < introTexts.Length; i++)
        {
            bool isActive = (i == index);
            introTexts[i].SetActive(isActive);

            // 텍스트 활성화 시 사운드 재생
            if (isActive && introTextSounds != null && i < introTextSounds.Length && introTextSounds[i] != null)
            {
                introTextSounds[i].Play();
            }
        }
    }

// 회전 속도에 따라 이동 방향으로 회전
    void RotateTowardsMovementDirection()
    {
        Vector3 velocity = agent.velocity;
        if (velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
