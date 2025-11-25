using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class Security_B : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Detection Settings")]
    public float detectionRange = 7f;      // 플레이어 탐지 범위
    public float blindDuration = 3f;       // 시야 마비 시간
    public float blindCooldown = 7f;       // 눈뽕 쿨다운
    private float lastBlindTime = -999f;

    [Tooltip("시야를 막는 레이어(벽/지형 등)")]
    public LayerMask obstacleMask;

    [Header("Flashlight Settings")]
    public Light flashLight;               // SpotLight
    public float flashlightIntensity = 5f;

    [Header("Audio")]
    public AudioSource blindSound;         // 눈뽕 효과음

    [Header("UI Settings")]
    public Image flashOverlay;             // 화면 밝아지는 효과

    [Header("Animation")]
    public Animator animator;
    public float speedDampTime = 0.1f; // 전환 부드럽게

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerController playerController;

    private bool isBlinding = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null) animator = GetComponent<Animator>();
        if (animator) animator.applyRootMotion = false; // 이동은 에이전트 담당

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        if (flashLight != null)
            flashLight.enabled = false;

        if (flashOverlay != null)
            flashOverlay.enabled = false;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private void Update()
    {
        UpdateAnimatorByAgent();

        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 플레이어가 범위 안 + 쿨다운 끝남 + 눈뽕 중 아님 + 시야 확보됨 → 발동
        if (distanceToPlayer <= detectionRange)
        {
            if (!isBlinding &&
                Time.time - lastBlindTime >= blindCooldown &&
                CanSeePlayer())
            {
                StartCoroutine(BlindPlayer());
            }
        }
        else
        {
            Patrol();
        }
    }

    // NavMeshAgent → Animator.Speed
    private void UpdateAnimatorByAgent()
    {
        if (animator == null || agent == null) return;

        float speed = agent.velocity.magnitude; // 실제 이동 속도(m/s)

        // 정지 판정이 살짝 떨리면 하드 클램프(선택):
        // if (!agent.hasPath || agent.remainingDistance <= 0.05f) speed = 0f;

        animator.SetFloat("Speed", speed, speedDampTime, Time.deltaTime);
    }

    /// <summary>
    /// 경비병이 플레이어를 직접 볼 수 있는가? (벽으로 가려지면 false)
    /// </summary>
    private bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.6f; // 경비병 눈 위치
        Vector3 target = playerTransform.position + Vector3.up * 1.6f;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        // 레이캐스트로 장애물 체크
        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, obstacleMask))
        {
            // 플레이어 말고 다른게 맞으면 장애물
            return false;
        }

        return true;
    }

    private IEnumerator BlindPlayer()
    {
        isBlinding = true;

        // 🔊 효과음 재생
        if (blindSound != null)
            blindSound.Play();

        // 손전등 ON
        if (flashLight != null)
            flashLight.enabled = true;

        // 오버레이 밝아짐
        if (flashOverlay != null)
        {
            flashOverlay.enabled = true;
            flashOverlay.color = new Color(1, 1, 1, 0);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 4f;
                float a = Mathf.Lerp(0f, 0.8f, t);
                flashOverlay.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
        }

        // 플레이어 조작 불가
        if (playerController != null)
            playerController.enabled = false;

        yield return new WaitForSeconds(blindDuration);

        // 손전등 OFF
        if (flashLight != null)
            flashLight.enabled = false;

        // 플레이어 조작 복구
        if (playerController != null)
            playerController.enabled = true;

        // 오버레이 서서히 사라짐
        if (flashOverlay != null)
        {
            float t = 0f;
            float startA = flashOverlay.color.a;

            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                float a = Mathf.Lerp(startA, 0f, t);
                flashOverlay.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }

            flashOverlay.enabled = false;
        }

        // 쿨다운 시작
        lastBlindTime = Time.time;

        isBlinding = false;
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0 || isBlinding) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }
}
