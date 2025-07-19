using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 지정한 박스 영역 안에 여러 Prefab들을 랜덤 배치.
/// - Inspector에서 itemPrefabs 리스트에 Prefab을 넣거나
/// - SpawnItems() 를 직접 다시 호출하여 재스폰 가능.
/// </summary>
public class RandomItemSpawner : MonoBehaviour
{
    [Header("Spawn Area (Local Box Size)")]
    [Tooltip("스폰할 박스의 가로(X) / 높이(Y) / 깊이(Z) *전체* 크기 (센터는 이 오브젝트)")]
    public Vector3 areaSize = new Vector3(10f, 5f, 10f);

    [Header("Prefabs Pool")]
    [Tooltip("랜덤으로 뽑을 Prefab 목록")]
    public List<GameObject> itemPrefabs = new();

    [Header("Spawn Settings")]
    [Tooltip("스폰할 총 개수")]
    public int spawnCount = 20;
    [Tooltip("재생(Play) 시작 시 자동 스폰할지 여부")]
    public bool spawnOnStart = true;

    [Header("Overlap / Distance Avoidance")]
    [Tooltip("겹침 회피 기능 사용 여부")]
    public bool avoidOverlap = true;
    [Tooltip("이미 스폰된 위치와의 최소 거리")]
    public float minDistance = 1.0f;
    [Tooltip("Physics.CheckSphere로 겹침 검사할 반경")]
    public float overlapCheckRadius = 0.4f;
    [Tooltip("겹침 검사 시 포함할 레이어 (아이템 Prefab들이 속한 레이어 설정)")]
    public LayerMask overlapCheckLayers = ~0; // 기본 전체

    [Header("Debug / Runtime")]
    [Tooltip("실제로 스폰된 인스턴스 추적 리스트")]
    public List<GameObject> spawnedItems = new();

    // 필요시 고정 시드를 주고 싶으면 외부에서 설정한 후 UseSeed = true
    [Header("Random Seed (Optional)")]
    public bool useSeed = false;
    public int seed = 12345;

    void Start()
    {
        if (useSeed)
            Random.InitState(seed);

        if (spawnOnStart)
            SpawnItems();
    }

    /// <summary>
    /// 기존 것들 제거 후 새로 스폰
    /// </summary>
    [ContextMenu("Respawn")]
    public void SpawnItems()
    {
        ClearSpawned();

        if (itemPrefabs == null || itemPrefabs.Count == 0)
        {
            Debug.LogWarning("[RandomItemSpawner] itemPrefabs 리스트가 비어 있습니다.");
            return;
        }

        int spawned = 0;
        int safety = 0;
        int maxIterations = spawnCount * 30; // 무한 루프 방지

        while (spawned < spawnCount && safety < maxIterations)
        {
            safety++;

            // 로컬 박스 내부에서 랜덤 좌표 (중심 0,0,0 기준)
            Vector3 randomLocal = new Vector3(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f),
                Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f)
            );

            // 월드 좌표로 변환 (Spawner 오브젝트의 회전/스케일 반영)
            Vector3 worldPos = transform.TransformPoint(randomLocal);

            if (avoidOverlap)
            {
                // 1) 물리 겹침 검사
                if (Physics.CheckSphere(worldPos, overlapCheckRadius, overlapCheckLayers))
                    continue;

                // 2) 최소 거리 검사
                bool tooClose = false;
                foreach (var obj in spawnedItems)
                {
                    if (!obj) continue;
                    if (Vector3.Distance(obj.transform.position, worldPos) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;
            }

            // 랜덤 Prefab 선택 (균등)
            GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Count)];

            GameObject newItem = Instantiate(prefab, worldPos, Quaternion.identity, transform);
            spawnedItems.Add(newItem);
            spawned++;
        }

        if (spawned < spawnCount)
        {
            Debug.LogWarning($"[RandomItemSpawner] 요청 {spawnCount}개 중 {spawned}개만 스폰됨. " +
                             $"(공간이 좁거나 minDistance/overlap 조건이 너무 엄격할 수 있음)");
        }
    }

    /// <summary>
    /// 이미 스폰된 아이템 제거
    /// </summary>
    [ContextMenu("Clear Spawned")]
    public void ClearSpawned()
    {
        foreach (var go in spawnedItems)
        {
            if (go)
            {
                // 즉시 파괴 (에디터에서는 DestroyImmediate 써도 되지만 여기서는 Destroy)
                Destroy(go);
            }
        }
        spawnedItems.Clear();
    }

    /// <summary>
    /// 영역 Gizmo 표시
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.15f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, areaSize);

        Gizmos.color = new Color(0f, 0.7f, 1f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, areaSize);
    }
}
