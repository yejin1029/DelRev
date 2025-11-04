using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class AreaGaugeUI : MonoBehaviour
{
    public static AreaGaugeUI Instance { get; private set; }

    [Header("Gauge Elements")]
    public RectTransform arrowTransform;

    [Header("Angle Settings")]
    public float minAngle = -90f;
    public float maxAngle = 90f;

    [Header("Shake Settings")]
    [Tooltip("흔들림이 시작되는 임계 게이지 (%)")]
    public float shakeThreshold = 70f;
    [Tooltip("흔들림 강도 (각도 오프셋)")]
    public float shakeIntensity = 3f;
    [Tooltip("흔들림 속도 (진동 주기)")]
    public float shakeSpeed = 25f;

    private Coroutine shakeCoroutine;
    private float currentGauge = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ResetGauge();

    public void UpdateGaugeUI(float gaugePercent)
    {
        if (arrowTransform == null) return;
        currentGauge = gaugePercent;

        // 기본 회전 (왼→오른쪽)
        float normalized = Mathf.Clamp01(gaugePercent / 100f);
        float baseAngle = Mathf.Lerp(maxAngle, minAngle, normalized);

        arrowTransform.localRotation = Quaternion.Euler(0f, 0f, baseAngle);

        // 🔥 흔들림 조건 확인
        if (gaugePercent >= shakeThreshold)
        {
            if (shakeCoroutine == null)
                shakeCoroutine = StartCoroutine(ShakeArrow());
        }
        else
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
                // 원래 각도로 복귀
                arrowTransform.localRotation = Quaternion.Euler(0f, 0f, baseAngle);
            }
        }
    }

    IEnumerator ShakeArrow()
    {
        while (true)
        {
            if (arrowTransform == null) yield break;

            // 흔들림 오프셋 계산
            float shakeOffset = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;

            // 현재 게이지 기준 각도 재계산
            float normalized = Mathf.Clamp01(currentGauge / 100f);
            float baseAngle = Mathf.Lerp(maxAngle, minAngle, normalized);

            arrowTransform.localRotation = Quaternion.Euler(0f, 0f, baseAngle + shakeOffset);

            yield return null;
        }
    }

    public void ResetGauge()
    {
        UpdateGaugeUI(0f);
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
    }
}
