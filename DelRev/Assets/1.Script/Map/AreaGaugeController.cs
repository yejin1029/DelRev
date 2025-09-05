using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class AreaGaugeController : MonoBehaviour
{
    [Header("Gauge Settings (0~100)")]
    [Tooltip("1초당 게이지가 채워지는 속도")]
    public float fillSpeed = 20f;

    [Tooltip("1초당 게이지가 감소하는 속도")]
    public float drainSpeed = 10f;

    [Range(0f, 100f)]
    [SerializeField]
    private float currentGauge = 0f;

    [Header("Danger Callback")]
    [Tooltip("게이지가 100이 되면 호출할 몬스터 (IDangerTarget 필요)")]
    [SerializeField] private MonoBehaviour targetMonsterBehaviour; // Inspector에서 드래그
    private IDangerTarget targetMonster;

    [Header("Toggle Target (will blink at full gauge)")]
    [Tooltip("게이지가 100이 되는 순간부터 1초마다 활성/비활성될 오브젝트\n" +
             "에디터에서 지정하지 않으면 MainCanvas/Filter를 찾아 할당합니다.")]
    public GameObject toggleTarget;

    [Header("UI References")]
    public Image gaugeFillImage;

    [Header("Audio References")]
    public AudioSource onFillStartSource;
    public AudioSource onFullGaugeSource;

    // --- SafetyZone 글로벌 상태 ---
    public static bool PlayerInSafetyZone = false;

    // 내부 상태
    private bool hasEnteredOnce = false;
    private bool isInside = false;
    private bool hasTriggeredDanger = false;
    private Coroutine toggleCoroutine;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Awake()
    {
        // Inspector에서 드래그된 Behaviour를 IDangerTarget으로 변환
        if (targetMonsterBehaviour != null)
        {
            targetMonster = targetMonsterBehaviour as IDangerTarget;
            if (targetMonster == null)
            {
                Debug.LogError("AreaGaugeController: 드래그된 객체가 IDangerTarget을 구현하지 않았습니다!");
            }
        }
        else
        {
            Debug.LogWarning("AreaGaugeController: targetMonsterBehaviour가 설정되지 않았습니다. Inspector에서 몬스터를 연결하세요!");
        }

        // toggleTarget 자동 할당
        if (toggleTarget == null)
        {
            var mainCanvas = GameObject.Find("MainCanvas");
            if (mainCanvas != null)
            {
                var filter = mainCanvas.transform.Find("Filter");
                if (filter != null)
                {
                    toggleTarget = filter.gameObject;
                    Debug.Log("AreaGaugeController: toggleTarget을 MainCanvas/Filter로 자동 할당했습니다.");
                }
            }
        }
    }

    void Update()
    {
        if (!hasEnteredOnce) return;
        if (currentGauge >= 100f) return;

        if (isInside)
        {
            // 세이프티존 안 → 게이지 감소
            currentGauge -= drainSpeed * Time.deltaTime;
            if (onFillStartSource != null && onFillStartSource.isPlaying)
                onFillStartSource.Stop();
        }
        else
        {
            // 세이프티존 밖 → 게이지 증가
            currentGauge += fillSpeed * Time.deltaTime;
            if (onFillStartSource != null && !onFillStartSource.isPlaying)
                onFillStartSource.Play();
        }

        currentGauge = Mathf.Clamp(currentGauge, 0f, 100f);

        // UI 업데이트
        if (AreaGaugeUI.Instance != null)
            AreaGaugeUI.Instance.UpdateGaugeUI(currentGauge);
        else
            UpdateGaugeUI();

        // 게이지 100 도달 시 Danger 발동
        if (!hasTriggeredDanger && currentGauge >= 100f)
        {
            hasTriggeredDanger = true;

            if (onFullGaugeSource != null)
                onFullGaugeSource.Play();

            if (targetMonster != null)
            {
                Debug.Log("AreaGaugeController: DangerGauge Max → " + targetMonster);
                targetMonster.OnDangerGaugeMaxed();
            }
            else
            {
                Debug.LogWarning("AreaGaugeController: targetMonster가 연결되지 않아 Danger 호출 실패!");
            }

            // 화면 토글 효과
            if (toggleTarget != null)
                toggleCoroutine = StartCoroutine(ToggleTargetEverySecond());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!hasEnteredOnce)
            hasEnteredOnce = true;

        isInside = true;
        PlayerInSafetyZone = true; // 전역 표시
        Debug.Log("AreaGaugeController: Player SafetyZone 진입 → 게이지 감소 시작");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || !hasEnteredOnce) return;

        isInside = false;
        PlayerInSafetyZone = false; // 전역 표시
        Debug.Log("AreaGaugeController: Player SafetyZone 이탈 → 게이지 증가 시작");
    }

    void UpdateGaugeUI()
    {
        if (gaugeFillImage != null)
            gaugeFillImage.fillAmount = currentGauge / 100f;
    }

    private IEnumerator ToggleTargetEverySecond()
    {
        while (true)
        {
            toggleTarget.SetActive(!toggleTarget.activeSelf);
            yield return new WaitForSeconds(1f);
        }
    }

    public void StopToggling()
    {
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }
    }
}
