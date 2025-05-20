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
    [Tooltip("게이지가 100이 되면 이 몬스터의 위험 상태를 호출합니다")]
    public Mom targetMonster;

    [Header("Toggle Target (will blink at full gauge)")]
    [Tooltip("게이지가 100이 되는 순간부터 1초마다 활성/비활성될 오브젝트")]
    public GameObject toggleTarget;

    [Header("UI References")]
    public Image gaugeFillImage;

    [Header("Audio References")]
    public AudioSource onFillStartSource;
    public AudioSource onFullGaugeSource;

    private bool hasEnteredOnce = false;
    private bool isInside = false;
    private bool hasTriggeredDanger = false;
    private Coroutine toggleCoroutine;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (!hasEnteredOnce) return;
        if (currentGauge >= 100f) return;

        if (isInside)
        {
            currentGauge -= drainSpeed * Time.deltaTime;
            if (onFillStartSource != null && onFillStartSource.isPlaying)
                onFillStartSource.Stop();
        }
        else
        {
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

        // 게이지 100 도달 시 한 번만 실행
        if (!hasTriggeredDanger && currentGauge >= 100f)
        {
            hasTriggeredDanger = true;

            if (onFullGaugeSource != null)
                onFullGaugeSource.Play();

            if (targetMonster != null)
                targetMonster.OnDangerGaugeMaxed();
            else
                Debug.LogWarning("AreaGaugeController: targetMonster가 할당되지 않았습니다!");

            // 토글 코루틴 시작
            if (toggleTarget != null)
                toggleCoroutine = StartCoroutine(ToggleTargetEverySecond());
            else
                Debug.LogWarning("AreaGaugeController: toggleTarget을 할당해주세요!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!hasEnteredOnce)
        {
            hasEnteredOnce = true;
            Debug.Log("첫 진입: 게이지 로직 활성화 (현재 " + currentGauge + ")");
        }

        isInside = true;
        Debug.Log("영역 진입: 게이지 감소 시작");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || !hasEnteredOnce) return;

        isInside = false;
        Debug.Log("영역 이탈: 게이지 채우기 시작");
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

    /// <summary>
    /// 외부에서 원할 때 토글 중단
    /// </summary>
    public void StopToggling()
    {
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }
    }
}
