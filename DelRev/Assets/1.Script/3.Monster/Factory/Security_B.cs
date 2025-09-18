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
    public float detectionRange = 7f;    // 플레이어 탐지 범위
    public float blindDuration = 3f;     // 시야 마비 시간

    [Header("Flashlight Settings")]
    public Light flashLight;             // 손전등 (Spotlight)
    public float flashlightIntensity = 5f;

    [Header("UI Settings")]
    [Tooltip("플래시 효과를 띄울 오버레이 이미지 (SubCanvas 안 Image)")]
    public Image flashOverlay;           // 화면에 깔릴 이미지 (흰색 Image)

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerController playerController;

    private bool isBlinding = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        if (flashLight != null)
            flashLight.enabled = false; // 기본 꺼짐

        if (flashOverlay != null)
            flashOverlay.enabled = false; // 기본 비활성화

        // 순찰 시작
        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            // 플레이어 발견 → 눈뽕 시도
            if (!isBlinding)
                StartCoroutine(BlindPlayer());
        }
        else
        {
            Patrol();
        }
    }

    private IEnumerator BlindPlayer()
    {
        isBlinding = true;

        // 손전등 켜기
        if (flashLight != null)
            flashLight.enabled = true;

        // 플레이어 시야 오버레이 켜기
        if (flashOverlay != null)
        {
            flashOverlay.enabled = true;
            flashOverlay.color = new Color(1f, 1f, 1f, 0f); // 투명에서 시작
            // 점점 밝아지는 효과
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 4f;
                flashOverlay.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 0.8f, t));
                yield return null;
            }
        }

        // 플레이어 이동 불가
        if (playerController != null)
            playerController.enabled = false;

        Debug.Log("[Security_B] 플레이어 눈뽕! 시야 마비 시작");

        yield return new WaitForSeconds(blindDuration);

        // 손전등 끄기
        if (flashLight != null)
            flashLight.enabled = false;

        // 플레이어 복구
        if (playerController != null)
            playerController.enabled = true;

        // 오버레이 점점 사라지기
        if (flashOverlay != null)
        {
            float t = 0f;
            Color start = flashOverlay.color;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                flashOverlay.color = new Color(1f, 1f, 1f, Mathf.Lerp(start.a, 0f, t));
                yield return null;
            }
            flashOverlay.enabled = false;
        }

        Debug.Log("[Security_B] 시야 마비 해제");

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
