using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class AreaGaugeController : MonoBehaviour
{
    [Header("Gauge Settings (0~100)")]
    public float fillSpeed = 20f;
    public float drainSpeed = 10f;

    [Range(0f, 100f)]
    [SerializeField]
    private float currentGauge = 0f;

    [Header("Danger Callback")]
    [SerializeField] private MonoBehaviour targetMonsterBehaviour;
    private IDangerTarget targetMonster;

    [Header("Toggle Target (blink at full gauge)")]
    public GameObject toggleTarget;

    [Header("UI References")]
    public Image gaugeFillImage;

    [Header("Tick Audio")]
    public AudioSource tickAudioSource;
    public AudioClip tickClip;

    [Header("Danger Audio")]
    public AudioSource onFullGaugeSource;

    public static bool PlayerInSafetyZone = false;

    private bool hasEnteredOnce = false;
    private bool isInside = false;
    private bool hasTriggeredDanger = false;
    private Coroutine toggleCoroutine;
    private Coroutine tickCoroutine;


    // =====================================================================
    void Awake()
    {
        if (targetMonsterBehaviour != null)
        {
            targetMonster = targetMonsterBehaviour as IDangerTarget;
            if (targetMonster == null)
                Debug.LogError("AreaGaugeController: targetMonsterBehaviour가 IDangerTarget이 아님!");
        }

        if (toggleTarget == null)
        {
            var mainCanvas = GameObject.Find("MainCanvas");
            if (mainCanvas != null)
            {
                var filter = mainCanvas.transform.Find("Filter");
                if (filter != null)
                    toggleTarget = filter.gameObject;
            }
        }
    }

    void Start()
    {
        tickCoroutine = StartCoroutine(TickLoop());
    }

    void Update()
    {
        if (!hasEnteredOnce) return;

        // ★★★ 핵심 수정: Danger(100%) 상태에서는 절대 감소하지 않음!
        if (isInside && !hasTriggeredDanger)
            currentGauge -= drainSpeed * Time.deltaTime;
        else
            currentGauge += fillSpeed * Time.deltaTime;

        currentGauge = Mathf.Clamp(currentGauge, 0f, 100f);

        // UI 업데이트
        if (AreaGaugeUI.Instance != null)
            AreaGaugeUI.Instance.UpdateGaugeUI(currentGauge);
        else
            UpdateGaugeUI();

        // Danger Trigger
        if (!hasTriggeredDanger && currentGauge >= 100f)
        {
            hasTriggeredDanger = true;

            if (onFullGaugeSource != null)
                onFullGaugeSource.Play();

            if (targetMonster != null)
                targetMonster.OnDangerGaugeMaxed();

            if (toggleTarget != null)
                toggleCoroutine = StartCoroutine(ToggleTargetEverySecond());
        }
    }


    // =====================================================================
    // Tick Loop (Danger 이후에도 계속 유지)
    // =====================================================================
    private IEnumerator TickLoop()
    {
        while (true)
        {
            if (!isInside && currentGauge > 0f)
            {
                float volume = CalculateVolume(currentGauge);
                float interval = CalculateInterval(currentGauge);

                tickAudioSource.volume = volume;

                if (tickClip != null)
                    tickAudioSource.PlayOneShot(tickClip);

                yield return new WaitForSeconds(interval);
            }
            else
            {
                yield return null;
            }
        }
    }


    // =====================================================================
    // Volume (4단계 동일)
    // =====================================================================
    private float CalculateVolume(float gauge)
    {
        if (gauge <= 0f)
            return 0f;

        if (gauge <= 30f)
            return Mathf.Lerp(0f, 0.5f, gauge / 30f);

        if (gauge <= 70f)
            return Mathf.Lerp(0.5f, 0.7f, (gauge - 30f) / 40f);

        if (gauge <= 90f)
            return Mathf.Lerp(0.7f, 1f, (gauge - 70f) / 20f);

        return 1f;
    }


    // =====================================================================
    // Interval (4단계 — 90~100% → 0.3→0.2초)
    // =====================================================================
    private float CalculateInterval(float gauge)
    {
        if (gauge <= 0f)
            return 999f;

        if (gauge <= 30f)
            return 1.5f;

        if (gauge <= 70f)
            return Mathf.Lerp(1.5f, 0.7f, (gauge - 30f) / 40f);

        if (gauge <= 90f)
            return Mathf.Lerp(0.7f, 0.3f, (gauge - 70f) / 20f);

        return Mathf.Lerp(0.3f, 0.2f, (gauge - 90f) / 10f);
    }


    // =====================================================================
    // Trigger
    // =====================================================================
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        hasEnteredOnce = true;
        isInside = true;
        PlayerInSafetyZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || !hasEnteredOnce) return;

        isInside = false;
        PlayerInSafetyZone = false;
    }


    // =====================================================================
    // UI & Blink
    // =====================================================================
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
}
