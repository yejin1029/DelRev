using UnityEngine;

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
    public Mom targetMonster;  // Inspector에서 Mom 객체를 드래그하세요

    private bool hasEnteredOnce = false;
    private bool isInside = false;
    private bool hasTriggeredDanger = false;  // 한 번만 호출하기 위한 플래그

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (!hasEnteredOnce) return;

        // 영역 안/밖에 따라 게이지 증감
        if (isInside)
            currentGauge -= drainSpeed * Time.deltaTime;
        else
            currentGauge += fillSpeed * Time.deltaTime;

        currentGauge = Mathf.Clamp(currentGauge, 0f, 100f);

        // 게이지가 100 도달 시 한 번만 위험 상태 호출
        if (!hasTriggeredDanger && currentGauge >= 100f)
        {
            hasTriggeredDanger = true;
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
}
