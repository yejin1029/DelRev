using UnityEngine;
using UnityEngine.AI;

public class SmartKidAI : MonoBehaviour
{
    public Transform[] roamPoints;
    public float interactionRange = 3f;
    public float problemCooldown = 5f; // 🔹 문제 다시 출제되기 전 대기 시간 (초)

    private NavMeshAgent agent;
    private bool isPlayerLocked = false;
    private Transform player;

    private float lastProblemTime = -999f; // 🔹 마지막 문제 낸 시간 저장

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        GoToRandomPoint();
    }

    void Update()
    {
        if (isPlayerLocked || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 🔹 쿨다운 확인
        if (Time.time - lastProblemTime < problemCooldown) return;

        // 🔹 범위 안에 들어오면 문제 출제
        if (distanceToPlayer <= interactionRange)
        {
            LockPlayer();
            return;
        }

        // 🔹 목적지 도착 시 새로운 포인트 이동
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToRandomPoint();
        }
    }

    void GoToRandomPoint()
    {
        if (roamPoints.Length == 0) return;
        int randomIndex = Random.Range(0, roamPoints.Length);

        // NavMesh 위 좌표로 보정
        NavMeshHit hit;
        if (NavMesh.SamplePosition(roamPoints[randomIndex].position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log($"[SmartKid] 이동: {roamPoints[randomIndex].name} → 보정 좌표 {hit.position}");
        }
        else
        {
            Debug.LogWarning("[SmartKid] NavMesh에서 유효한 포인트를 찾지 못함!");
        }
    }


    void LockPlayer()
    {
        isPlayerLocked = true;
        agent.isStopped = true;
        Debug.Log("[SmartKid] 플레이어 발견 → 문제 출제 시작!");

        // 🔹 PlayerInputBlocker를 통해 조작 막기
        PlayerInputBlocker blocker = player.GetComponent<PlayerInputBlocker>();
        if (blocker != null) blocker.BlockInput();

        ProblemManager.Instance.StartProblem(player.gameObject, this);
    }

    public void ReleasePlayer()
    {
        isPlayerLocked = false;
        agent.isStopped = false;
        lastProblemTime = Time.time; // 🔹 마지막 문제 시간 갱신
        Debug.Log("[SmartKid] 플레이어 해방 → 다시 돌아다님 (쿨다운 시작)");

        // 🔹 PlayerInputBlocker를 통해 조작 복구
        PlayerInputBlocker blocker = player.GetComponent<PlayerInputBlocker>();
        if (blocker != null) blocker.UnblockInput();

        GoToRandomPoint();
    }
}
