using UnityEngine;
using UnityEngine.UI;

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
    [Tooltip("현재 채워진 게이지 양")]
    private float currentGauge = 0f;

    [Header("Danger Callback")]
    [Tooltip("게이지가 100이 되면 이 몬스터의 위험 상태를 호출합니다")]
    public Mom targetMonster;

    [Header("UI References")]
    public Image gaugeFillImage;

    [Header("Audio References")]
    [Tooltip("게이지가 올라가기 시작할 때 재생할 AudioSource")]
    public AudioSource onFillStartSource;

    [Tooltip("게이지가 100이 되었을 때 재생할 AudioSource")]
    public AudioSource onFullGaugeSource;

    private bool hasEnteredOnce = false;
    private bool isInside = false;
    private bool hasTriggeredDanger = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (!hasEnteredOnce) return;

        // 게이지가 이미 100이면 증감 멈춤
        if (currentGauge >= 100f)
            return;

        if (isInside)
        {
            // 안전 구역: 게이지 감소
            currentGauge -= drainSpeed * Time.deltaTime;

            // 증가 시작 오디오 중지
            if (onFillStartSource != null && onFillStartSource.isPlaying)
                onFillStartSource.Stop();
        }
        else
        {
            // 영역 밖: 게이지 증가
            currentGauge += fillSpeed * Time.deltaTime;

            // 증가 시작 오디오 재생
            if (onFillStartSource != null && !onFillStartSource.isPlaying)
                onFillStartSource.Play();
        }

        currentGauge = Mathf.Clamp(currentGauge, 0f, 100f);

        // UI 업데이트
        if (AreaGaugeUI.Instance != null)
            AreaGaugeUI.Instance.UpdateGaugeUI(currentGauge);
        else
            UpdateGaugeUI();

        // 게이지가 100 도달 시 한 번만 위험 상태 호출 및 풀 게이지 오디오 재생
        if (!hasTriggeredDanger && currentGauge >= 100f)
        {
            hasTriggeredDanger = true;

            // onFillStartSource는 계속 재생되도록 Stop 호출 제거

            if (onFullGaugeSource != null)
                onFullGaugeSource.Play();

            if (targetMonster != null)
                targetMonster.OnDangerGaugeMaxed();
            else
                Debug.LogWarning("AreaGaugeController: targetMonster가 할당되지 않았습니다!");
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

    // 기존 UI 업데이트 (fallback)
    void UpdateGaugeUI()
    {
        if (gaugeFillImage != null)
            gaugeFillImage.fillAmount = currentGauge / 100f;
    }
}
