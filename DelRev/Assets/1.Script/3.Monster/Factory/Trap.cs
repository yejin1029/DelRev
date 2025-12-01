using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Trap : MonoBehaviour
{
    [Header("Trap Settings")]
    public float disableDuration = 1f;        // 이동 불가 시간
    public float activationDistance = 1f;     // 발동 거리
    public AudioClip trapSound;               // 덫 소리 (선택)

    [Header("Animation")]
    public Animator animator;
    public string activateTrigger = "Activate"; // AC 파라미터 이름

    private Transform playerTransform;
    private bool isTriggered = false;
    private AudioSource audioSource;

    [Header("UI (Screen Flash)")]
    [SerializeField] private GameObject blackImageObj;
    [SerializeField] private float blinkInterval = 0.1f; // 깜빡 주기

    void Start()
    {
        // 오디오소스 자동 세팅
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 애니메이터 자동 할당
        if (animator == null) animator = GetComponent<Animator>();

        // Player 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        // ★ 인스펙터에서 비어있으면, 비활성 포함 검색으로 보강
        if (blackImageObj == null)
        {
            // 경로로 먼저 시도
            var byPath = GameObject.Find("SubCanvas/Black") ?? GameObject.Find("Black");
            if (byPath != null) blackImageObj = byPath;
            else
            {
                // 비활성 포함 검색 (Unity 2021↑은 Resources.FindObjectsOfTypeAll 사용)
                foreach (var img in Resources.FindObjectsOfTypeAll<Image>())
                {
                    if (img.gameObject.name == "Black")
                    {
                        blackImageObj = img.gameObject;
                        break;
                    }
                }
            }
        }

        if (blackImageObj != null)
        {
            // 시작은 꺼둠
            blackImageObj.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Trap: 'Black' UI 오브젝트를 찾을 수 없습니다. 인스펙터에 직접 할당하세요.");
        }
    }

    void Update()
    {
        if (isTriggered || playerTransform == null) return;

        float dist = Vector3.Distance(playerTransform.position, transform.position);
        if (dist <= activationDistance)
        {
            PlayerController player = playerTransform.GetComponentInChildren<PlayerController>();
            if (player != null)
            {
                isTriggered = true;

                if (trapSound != null)
                    audioSource.PlayOneShot(trapSound);

                // 발동 애니메이션 트리거
                if (animator != null)
                    animator.SetTrigger(activateTrigger);

                // UI 깜빡이기 & 플레이어 잠금
                StartCoroutine(DisableMovement(player));
                StartCoroutine(BlinkBlackFor(disableDuration)); // 깜빡임 시작
            }
        }
    }

    private IEnumerator DisableMovement(PlayerController player)
    {
        player.enabled = false;
        yield return new WaitForSeconds(disableDuration);
        player.enabled = true;

        Destroy(gameObject);
    }

    // 깜빡임을 덫 지속시간과 동기화
    private IEnumerator BlinkBlackFor(float duration)
    {
        if (blackImageObj == null) yield break;

        float elapsed = 0f;
        bool on = false;

        // WaitForSecondsRealtime 사용: 타임스케일 0이어도 정확히 진행
        while (elapsed < duration)
        {
            on = !on;
            blackImageObj.SetActive(on);
            yield return new WaitForSecondsRealtime(blinkInterval);
            elapsed += blinkInterval;
        }

        // 종료 시 확실히 끄기
        blackImageObj.SetActive(false);
    }
}
