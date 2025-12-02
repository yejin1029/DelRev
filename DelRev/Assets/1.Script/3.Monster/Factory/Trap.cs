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

        // 인스펙터에서 비어있으면, 비활성 포함 검색으로 보강
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
            var player = playerTransform.GetComponentInChildren<PlayerController>();
            if (player == null) return;

            isTriggered = true;

            if (trapSound != null) audioSource.PlayOneShot(trapSound);
            if (animator != null) animator.SetTrigger(activateTrigger);

            // 하나의 코루틴에서 모든 시퀀스 보장
            StartCoroutine(TrapSequence(player));
        }
    }

    // 깜빡임 + 이동제한 + 정리까지 한 번에
    private IEnumerator TrapSequence(PlayerController player)
    {
        // 1) 플레이어 이동 제한
        player.enabled = false;

        // 2) disableDuration 동안 깜빡임 (Realtime)
        if (blackImageObj != null)
        {
            float elapsed = 0f;
            bool on = false;

            while (elapsed < disableDuration)
            {
                on = !on;
                blackImageObj.SetActive(on);

                float step = Mathf.Min(blinkInterval, disableDuration - elapsed);
                yield return new WaitForSecondsRealtime(step);
                elapsed += step;
            }

            // 3) 반드시 끄기
            blackImageObj.SetActive(false);
        }
        else
        {
            // UI가 없더라도 시간은 보장
            yield return new WaitForSecondsRealtime(disableDuration);
        }

        // 4) 플레이어 복구
        player.enabled = true;

        // 5) 마지막에 덫 파괴 (여기서 코루틴 종료되어도 이미 Black은 꺼짐)
        Destroy(gameObject);
    }

    // 안전장치: 오브젝트가 중도 파괴/비활성화돼도 화면을 꺼둔다
    private void OnDisable()
    {
        if (blackImageObj != null) blackImageObj.SetActive(false);
    }

    private void OnDestroy()
    {
        if (blackImageObj != null) blackImageObj.SetActive(false);
    }
}