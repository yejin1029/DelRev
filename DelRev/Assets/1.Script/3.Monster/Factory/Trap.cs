using UnityEngine;
using System.Collections;

public class Trap : MonoBehaviour
{
    [Header("Trap Settings")]
    public float disableDuration = 5f;        // 이동 불가 시간
    public float activationDistance = 1f;     // 발동 거리

    private Transform playerTransform;
    private bool isTriggered = false;

    void Start()
    {
        // Player 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Trap: 'Player' 태그의 오브젝트를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        if (isTriggered || playerTransform == null) return;

        // 거리 계산
        float dist = Vector3.Distance(playerTransform.position, transform.position);
        if (dist <= activationDistance)
        {
            // PlayerController 가져오기 (자식까지 포함)
            PlayerController player = playerTransform.GetComponentInChildren<PlayerController>();
            if (player != null)
            {
                Debug.Log("[Trap] 플레이어가 덫 밟음 → " + disableDuration + "초간 이동 불가");
                StartCoroutine(DisableMovement(player));
                isTriggered = true; // 중복 실행 방지
            }
        }
    }

    private IEnumerator DisableMovement(PlayerController player)
    {
        // 이동 막기
        player.enabled = false;

        // 대기
        yield return new WaitForSeconds(disableDuration);

        // 이동 복구
        player.enabled = true;
        Debug.Log("[Trap] 플레이어 이동 가능");

        // Trap 파괴 (코루틴 끝난 후 → 안전)
        Destroy(gameObject);
    }
}
