using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NearbyItemUI : MonoBehaviour
{
    public static NearbyItemUI Instance;

    [Header("Detection Settings")]
    [Tooltip("감지할 아이템 태그 이름")]
    public string itemTag = "Item";

    [Tooltip("플레이어 기준 감지 반경")]
    public float detectionRadius = 5f;

    [Tooltip("몇 초마다 갱신할지 (성능용)")]
    public float updateInterval = 0.2f;

    [Tooltip("감지할 레이어 (기본: 전체)")]
    public LayerMask detectionLayers = ~0; // Everything

    [Header("References")]
    [Tooltip("자동으로 못 찾으면 여기 수동 할당 가능")]
    public Transform playerTransform;

    [Tooltip("근처 아이템 개수를 표시할 TMP 텍스트")]
    public TextMeshProUGUI countText;

    private float timer = 0f;

    private void Awake()
    {
        // 싱글톤 + DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        FindPlayer();
        UpdateUI(0);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 바뀔 때마다 새 Player 찾기
        FindPlayer();
    }

    private void FindPlayer()
    {
        // PlayerController 싱글톤 먼저 시도
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        else
        {
            // 혹시 모를 경우 FindObjectOfType로 보조
            var player = FindObjectOfType<PlayerController>();
            playerTransform = player != null ? player.transform : null;
        }

        // 플레이어가 없으면 UI 숨김 (스타트씬, 게임오버씬 등)
        if (countText != null)
            countText.gameObject.SetActive(playerTransform != null);
    }

    private void Update()
    {
        // 플레이어 없으면 계속 시도만 하고 리턴
        if (playerTransform == null)
        {
            FindPlayer();
            timer = 0f;
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = updateInterval;
            int count = CountNearbyItems();
            UpdateUI(count);
        }
    }

    private int CountNearbyItems()
    {
        int count = 0;

        // 플레이어 주변 구 형태로 콜라이더 탐색
        Collider[] hits = Physics.OverlapSphere(
            playerTransform.position,
            detectionRadius,
            detectionLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (var hit in hits)
        {
            if (hit != null && hit.CompareTag(itemTag))
                count++;
        }

        return count;
    }

    private void UpdateUI(int count)
    {
        if (countText == null) return;

        countText.gameObject.SetActive(playerTransform != null);
        countText.text = $"아이템 감지 시스템 : {count}개";
    }

    // 씬 뷰에서 감지 범위 보이게 (디버그용)
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerTransform.position, detectionRadius);
    }
}
