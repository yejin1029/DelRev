using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// Monster Follow route with chase & attack sounds and chase-sound cooldown
[RequireComponent(typeof(NavMeshAgent))]
public class Mom : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPatrolIndex;

    public GameObject[] introTexts;

    private enum State { None, Patrol, Chase, Return, Alert }
    private State currentState;
    private State previousState;                // 이전 프레임 상태 저장용

    private Vector3 lastPatrolPosition;
    private Transform playerTransform;
    private NavMeshAgent agent;

    [Header("Intro Settings")]
    public Transform[] approachPoints; // 이동할 위치 배열
    private float waitDistance = 1f; // 플레이어가 얼마나 가까이 와야 기다림이 끝나는지
    private float checkInterval = 0.5f; // 플레이어 거리 체크 간격

    public Transform lastPatrolPoint = null;
    private bool hasDoneIntro = false;

    [Header("Combat Settings")]
    public float detectionRange = 3f;
    public float attackRange = 2f;
    public float damageAmount = 30f;     // 1초당 데미지
    public float damageInterval = 1f;    // 1초마다
    private float damageTimer = 0f;
    private PlayerController playerController;

    [Header("Sound Sources")]
    [Tooltip("추격 시작 시 재생할 AudioSource")]
    public AudioSource chaseSource;
    [Tooltip("공격할 때마다 재생할 AudioSource")]
    public AudioSource attackSource;

    [Header("Chase Sound Cooldown")]
    [Tooltip("추격 사운드 재생 후 재생되지 않도록 대기할 시간(초)")]
    public float chaseSoundCooldown = 7f;
    private float chaseSoundTimer = 0f;

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

        GameObject[] allTexts = FindObjectsOfType<GameObject>(true); // 비활성화 포함

        foreach (GameObject obj in allTexts)
        {
            if (obj.name == "Mom_Text1") introTexts[0] = obj;
            if (obj.name == "Mom_Text2") introTexts[1] = obj;
            if (obj.name == "Mom_Text3") introTexts[2] = obj;
        }

        // 모든 텍스트 비활성화
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
        if (currentState == State.None)
        {
            return;
        }

        if (playerTransform == null || playerController == null)
            return;

        // chase-sound 쿨다운 타이머 감소
        if (chaseSoundTimer > 0f)
            chaseSoundTimer -= Time.deltaTime;

        // 이전 상태 저장
        previousState = currentState;

        // Shift (Ctrl) 키 체크
        isShiftPressed = Input.GetKey(KeyCode.LeftControl);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 상태 머신
        switch (currentState)
        {
            case State.Patrol:
                if (!(isShiftPressed && distanceToPlayer > 2f))
                {
                    if (distanceToPlayer < detectionRange && HasLineOfSight())
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
                if (distanceToPlayer > 2f && !HasLineOfSight())
                {
                    currentState = State.Patrol;
                    GoToNextPatrolPoint();
                }
                else if (distanceToPlayer < detectionRange && HasLineOfSight())
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

        // 상태 전이 감지 & 추격 시작 사운드 재생 (쿨다운 체크)
        if (currentState == State.Chase
            && previousState != State.Chase
            && chaseSoundTimer <= 0f)
        {
            chaseSource?.Play();
            chaseSoundTimer = chaseSoundCooldown;
        }

        // 공격 범위 내 데미지 + 공격 사운드
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
        agent.speed = 5f;
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

    //문 여는 코드
    private void CheckForDoorAndInteract()
    {
        RaycastHit hit;
        Vector3 forward = transform.forward;

        // 문을 감지할 거리
        float checkDistance = 3.0f;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, forward, out hit, checkDistance))
        {
            SuburbanHouse.Door door = hit.collider.GetComponent<SuburbanHouse.Door>();
            if (door != null)
            {
                door.OpenDoorForMonster(); // 문 열기
            }
        }
    }

    IEnumerator IntroApproachThenReturn()
    {
        // 순차적으로 각 지점으로 이동하면서 플레이어를 기다림
        for (int i = 0; i < approachPoints.Length; i++)
        {
            Vector3 point = approachPoints[i].position;
            agent.SetDestination(point);

            // 오브젝트가 해당 위치로 이동할 때까지 대기
            while (Vector3.Distance(transform.position, point) > 1f)
            {
                CheckForDoorAndInteract();
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    break;
                yield return null;
            }

            // 플레이어가 해당 지점에 가까이 올 때까지 대기
            while (Vector3.Distance(playerTransform.position, point) > waitDistance)
            {
                yield return new WaitForSeconds(checkInterval);
            }

            // 텍스트 설정
            SetIntroText(i);

            lastPatrolPoint = approachPoints[i];
        }

        // 마지막 지점 도달 + 플레이어도 근처에 있는 상태
        if (lastPatrolPoint != null)
        {
            Vector3 point = lastPatrolPoint.position;
            agent.SetDestination(point);
            yield return new WaitForSeconds(2f);
            // 마지막 지점이면 텍스트 끄고 종료
            SetIntroText(-1); // 모두 끄기
        }

        yield return new WaitForSeconds(1f); // 다음 지점 이동 전 살짝 대기

        currentState = State.Patrol;
        GoToNextPatrolPoint();

        yield break;
    }

    void SetIntroText(int index)
    {
        for (int i = 0; i < introTexts.Length; i++)
        {
            introTexts[i].SetActive(i == index);
        }
    }
}