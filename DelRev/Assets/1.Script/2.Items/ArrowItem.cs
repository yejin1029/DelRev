using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSequenceController : MonoBehaviour
{
    [Header("Activation Distance")]
    [Tooltip("Trigger activation when player is within this distance")]  
    public float activationDistance = 1f;

    [Header("Items in sequence")]
    [Tooltip("Drag your arrows here in order (arrow1, arrow2, ...)")]
    public List<GameObject> items;

    [Header("Bobbing Settings")]
    [Tooltip("Vertical bob amplitude")]
    public float bobAmount = 0.3f;
    [Tooltip("Bob speed, cycles per second")]
    public float bobSpeed = 1f;

    [Header("Canvas Effects")]
    [Tooltip("첫 번째 화살표 먹었을 때 2초간 표시할 오브젝트")]
    public GameObject firstArrowEffect;
    [Tooltip("마지막 화살표 먹었을 때 2초간 표시할 오브젝트")]
    public GameObject lastArrowEffect;
    [Tooltip("이펙트가 켜져 있을 시간(초)")]
    public float effectDuration = 2f;

    [Header("Sound")]
    [Tooltip("화살표를 먹을 때 재생할 사운드 클립")]
    public AudioClip pickSound;
    [Range(0f, 1f)]
    [Tooltip("픽업 사운드 볼륨 (0~1)")]
    public float pickVolume = 1f;

    private int currentIndex = 0;
    private Vector3[] originalPositions;
    private Transform playerTransform;

    void Start()
    {
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning("ItemSequenceController: 'Player' 태그의 오브젝트를 찾을 수 없습니다.");

        // 원래 위치 저장 및 첫 번째 아이템만 활성화
        originalPositions = new Vector3[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null)
            {
                originalPositions[i] = items[i].transform.position;
                items[i].SetActive(i == currentIndex);
            }
        }

        // 효과 오브젝트 초기 비활성화
        if (firstArrowEffect != null)
            firstArrowEffect.SetActive(false);
        if (lastArrowEffect != null)
            lastArrowEffect.SetActive(false);
    }

    void Update()
    {
        if (currentIndex >= items.Count) return;

        float t = Time.time;

        GameObject item = items[currentIndex];
        if (item != null && item.activeSelf)
        {
            Vector3 pos = originalPositions[currentIndex];
            pos.y += Mathf.Sin(t * bobSpeed) * bobAmount;
            item.transform.position = pos;

            // 플레이어 거리 계산
            if (playerTransform != null)
            {
                float dist = Vector3.Distance(playerTransform.position, pos);
                if (dist <= activationDistance)
                {
                    // 🔊 사운드 재생
                    if (pickSound != null)
                    {
                        // 아이템 위치 기준 3D 사운드
                        AudioSource.PlayClipAtPoint(pickSound, item.transform.position, pickVolume);
                    }

                    // 현재 아이템 비활성화
                    item.SetActive(false);

                    // 🔹 첫 번째 화살표 먹었을 때 효과 표시
                    if (currentIndex == 0 && firstArrowEffect != null)
                        StartCoroutine(ShowEffect(firstArrowEffect));

                    // 🔹 마지막 화살표 먹었을 때 효과 표시
                    if (currentIndex == items.Count - 1 && lastArrowEffect != null)
                        StartCoroutine(ShowEffect(lastArrowEffect));

                    // 다음 아이템 활성화
                    currentIndex++;
                    if (currentIndex < items.Count && items[currentIndex] != null)
                        items[currentIndex].SetActive(true);
                }
            }
        }
    }

    IEnumerator ShowEffect(GameObject effectObj)
    {
        effectObj.SetActive(true);
        yield return new WaitForSeconds(effectDuration);
        effectObj.SetActive(false);
    }
}
