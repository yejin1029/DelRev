using UnityEngine;

/// <summary>
/// 카메라로 조준 중일 때 E를 누르면 지정한 PlaneItemToCoin.StartProcess() 실행.
/// 카메라를 직접 지정하지 않으면 Player 태그 하위의 카메라를 자동으로 찾는다.
/// </summary>
[DisallowMultipleComponent, RequireComponent(typeof(Collider))]
public class Lever : MonoBehaviour
{
    [Header("Target Converter")]
    [Tooltip("아이템 변환을 수행할 PlaneItemToCoin")]
    public PlaneItemToCoin converter;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Raycast")]
    [Tooltip("조준 판정을 위한 카메라 (비우면 Player 태그 → 자식 카메라 → Camera.main 순서로 탐색)")]
    public Camera viewCamera;
    [Tooltip("조준 허용 최대 거리")]
    public float maxDistance = 3f;
    [Tooltip("히트시킬 레이어 (모든 레이어면 기본값 유지)")]
    public LayerMask hitMask = ~0;

    [Header("Options")]
    [Tooltip("converter에 대기 중인 아이템이 있을 때만 작동")]
    public bool requireItemsInConverter = true;

    void Awake()
    {
        // 카메라 자동 탐색 로직
        if (viewCamera == null)
        {
            // 1️⃣ Player 태그 오브젝트에서 먼저 찾기
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                viewCamera = playerObj.GetComponentInChildren<Camera>();
                if (viewCamera != null)
                    Debug.Log("[Lever] Player 자식 카메라 자동 연결됨: " + viewCamera.name);
            }

            // 2️⃣ 못 찾았으면 Camera.main 시도
            if (viewCamera == null)
                viewCamera = Camera.main;

            if (viewCamera == null)
                Debug.LogWarning("[Lever] 카메라를 찾지 못했습니다. 수동 할당 필요.");
        }

        if (converter == null)
            Debug.LogWarning("[Lever] converter가 비어있습니다. 인스펙터에서 연결하세요.");
    }

    void Update()
    {
        if (converter == null || viewCamera == null) return;

        if (Input.GetKeyDown(interactKey) && IsAimingAtMe())
        {
            if (!requireItemsInConverter || converter.HasQueuedItems)
                converter.StartProcess();
        }
    }

    // 카메라 중앙 레이가 "나(또는 내 자식 콜라이더)"를 맞췄는지
    private bool IsAimingAtMe()
    {
        Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Collide))
        {
            if (hit.collider == null) return false;
            return hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform);
        }
        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var cam = viewCamera != null ? viewCamera : Camera.main;
        if (cam == null) return;
        Gizmos.color = Color.yellow;
        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
    }
#endif
}
