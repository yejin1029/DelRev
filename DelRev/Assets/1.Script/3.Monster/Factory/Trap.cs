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

    // 👇 SubCanvas/Black 이미지 참조
    private GameObject blackImageObj;

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

        // SubCanvas 안의 "Black" 이미지 찾기
        blackImageObj = GameObject.Find("Black");
        if (blackImageObj != null)
            blackImageObj.SetActive(false); // 시작 시 꺼두기
        else
            Debug.LogWarning("Trap: 'Black' 이미지 오브젝트를 찾을 수 없습니다.");
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
                StartCoroutine(BlinkBlackFor(disableDuration)); // ← 지속 시간을 덫 지속시간에 맞춤
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

        float t = 0f;
        float interval = 0.1f; // 한 번 on/off 주기
        bool on = false;

        while (t < duration)
        {
            on = !on;
            blackImageObj.SetActive(on);
            yield return new WaitForSeconds(interval);
            t += interval;
        }

        blackImageObj.SetActive(false);
    }
}
