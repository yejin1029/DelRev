using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class PlaneItemToCoin : MonoBehaviour
{
    [Header("Item Detection")]
    [Tooltip("감지할 아이템 태그")]
    public string itemTag = "Item";

    [Header("Rotation Target (X only)")]
    [Tooltip("회전시킬 피벗/부모 Transform (부모 배치 추천)")]
    public Transform rotatingPart;

    [Header("Rotation Path (X only)")]
    [Tooltip("시작/복귀 각도를 현재 X값으로 사용할지 (true 권장)")]
    public bool useCurrentAsRestX = true;
    [Range(-180, 180)] public float restX = 45f;  // useCurrentAsRestX=false일 때 시작/복귀 각
    [Range(-180, 180)] public float topX  = 90f;  // position1 목표값
    [Range(-180, 180)] public float lowX  = 0f;   // position2 목표값

    [Header("Rotation Timing (seconds)")]
    [Tooltip("position1: rest(or current) → topX 소요 시간")]
    public float position1Time = 0.5f;
    [Tooltip("position2: topX → lowX 소요 시간")]
    public float position2Time = 0.7f;
    [Tooltip("return: lowX → rest(or current) 소요 시간")]
    public float returnTime = 0.5f;

    [Header("Coin Delay & Audio")]
    [Tooltip("E를 누른 ‘그 순간’부터 지연 시작 (회전과 동시 진행)")]
    public float coinDelay = 1f;
    [Tooltip("기계 작동 소리 (즉시 재생)")]
    public AudioSource machineAudio;
    [Tooltip("코인 획득 소리 (coinDelay 후 재생)")]
    public AudioSource coinAudio;

    // 읽기 전용 상태
    public bool HasQueuedItems => itemsInZone.Count > 0;
    public bool IsProcessing => isProcessing;

    private PlayerController playerController;
    private readonly List<Item> itemsInZone = new List<Item>();
    private bool isProcessing = false;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerController = playerObj.GetComponent<PlayerController>();
        else Debug.LogError("[PlaneItemToCoin] Player 태그를 찾지 못했습니다.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(itemTag)) return;

        var item = other.GetComponent<Item>();
        if (item != null && !itemsInZone.Contains(item))
            itemsInZone.Add(item);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(itemTag)) return;

        var item = other.GetComponent<Item>();
        if (item != null && itemsInZone.Contains(item))
            itemsInZone.Remove(item);
    }

    /// <summary>
    /// 레버/크로스헤어 등 외부에서 호출. E를 누른 "그 순간"부터 coinDelay 카운트 시작.
    /// </summary>
    public void StartProcess()
    {
        if (isProcessing) return;
        if (!HasQueuedItems) return;

        float startedAt = Time.unscaledTime;      // ← E를 누른 시각
        StartCoroutine(ProcessAllItems(startedAt));
    }

    private IEnumerator ProcessAllItems(float startedAt)
    {
        isProcessing = true;

        // ① 즉시 기계 소리
        if (machineAudio) machineAudio.Play();

        // ② 회전과 coinDelay 처리를 병렬로 진행
        bool rotDone = false, coinDone = false;

        if (rotatingPart) StartCoroutine(RotateSequence_XOnly(rotatingPart, () => rotDone = true));
        StartCoroutine(CoinDelayProcess(startedAt, () => coinDone = true));

        // 둘 다 끝날 때까지 대기
        while (!(rotDone && coinDone)) yield return null;

        isProcessing = false;
    }

    private IEnumerator CoinDelayProcess(float startedAt, System.Action onDone)
    {
        // E를 누른 순간부터 coinDelay 보장
        float elapsedSincePress = Time.unscaledTime - startedAt;
        float remain = Mathf.Max(0f, coinDelay - elapsedSincePress);
        if (remain > 0f) yield return new WaitForSeconds(remain);

        // 코인 사운드
        if (coinAudio) coinAudio.Play();

        // 아이템 정산
        int total = 0;
        int count = itemsInZone.Count;
        foreach (var it in itemsInZone)
        {
            if (it == null) continue;
            total += it.itemPrice;
            Destroy(it.gameObject);
        }

        if (playerController && total > 0)
        {
            playerController.AddCoins(total);
            Debug.Log($"[PlaneItemToCoin] {count}개 아이템 → +{total} 코인 지급");
        }

        itemsInZone.Clear();
        onDone?.Invoke();
    }

    /// <summary>
    /// X축만: (restX or 현재X) → topX → lowX → restX 복귀.
    /// Y/Z는 시작 시점의 값 고정. 구간별 시간 각각 사용.
    /// </summary>
    private IEnumerator RotateSequence_XOnly(Transform target, System.Action onDone)
    {
        Vector3 startEuler = target.localEulerAngles;
        float fixedY = startEuler.y, fixedZ = startEuler.z;

        float startX = useCurrentAsRestX ? startEuler.x : restX;
        float backX  = useCurrentAsRestX ? startEuler.x : restX;

        float t1 = Mathf.Max(0.0001f, position1Time);
        float t2 = Mathf.Max(0.0001f, position2Time);
        float t3 = Mathf.Max(0.0001f, returnTime);

        yield return AnimateX(target, startX, topX, t1, fixedY, fixedZ);
        yield return AnimateX(target, topX,  lowX, t2, fixedY, fixedZ);
        yield return AnimateX(target, lowX,  backX, t3, fixedY, fixedZ);

        target.localEulerAngles = new Vector3(backX, fixedY, fixedZ);
        onDone?.Invoke();
    }

    // X만 보간, Y/Z 고정
    private IEnumerator AnimateX(Transform t, float fromX, float toX, float dur, float y, float z)
    {
        float acc = 0f;
        while (acc < dur)
        {
            float x = Mathf.LerpAngle(fromX, toX, acc / dur);
            t.localEulerAngles = new Vector3(x, y, z);
            acc += Time.deltaTime;
            yield return null;
        }
        t.localEulerAngles = new Vector3(toX, y, z);
    }
}
