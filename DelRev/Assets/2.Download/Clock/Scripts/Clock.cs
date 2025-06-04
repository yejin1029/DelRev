using UnityEngine;

public class Clock : MonoBehaviour
{
    public int minutes = 0;
    public int hour = 0;
    public int seconds = 0;
    public bool realTime = true;
    public GameObject pointerMinutes;
    public GameObject pointerHours;
    public float clockSpeed = 32.0f;

    float msecs = 0;
    bool safetyZoneUpdated = false; // ✅ 한 번만 적용되도록 체크

    void Start()
    {
        safetyZoneUpdated = false;
        hour = 9;
        minutes = 0;
        seconds = 0;
    }

    void Update()
    {
        //-- 시간 계산
        msecs += Time.deltaTime * clockSpeed;
        if (msecs >= 1.0f)
        {
            msecs -= 1.0f;
            seconds++;
            if (seconds >= 60)
            {
                seconds = 0;
                minutes++;
                if (minutes >= 60)
                {
                    minutes = 0;
                    hour++;
                    if (hour >= 24)
                        hour = 0;
                }
            }
        }

        //-- 시계 바늘 회전
        float rotationMinutes = 360.0f / 60.0f * minutes;
        float rotationHours = (360.0f / 12.0f * hour) + (360.0f / (60.0f * 12.0f) * minutes);
        pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);
        pointerHours.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationHours);

        //-- ✅ 18:00이 되면 SafetyZone 설정
        if (hour == 18 && minutes == 0 && seconds == 0)
        {
            GameObject safetyZone = GameObject.Find("SafetyZone");
            if (safetyZone != null)
            {
                AreaGaugeController controller = safetyZone.GetComponent<AreaGaugeController>();
                if (controller != null)
                {
                    controller.fillSpeed = 10f;
                    controller.drainSpeed = -10f;
                    Debug.Log("🟢 18:00 - SafetyZone 설정 완료!");
                    safetyZoneUpdated = true; // 한 번만 실행되도록
                }
                else
                {
                    Debug.LogWarning("⚠️ SafetyZone에 AreaGaugeController 컴포넌트가 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ 'SafetyZone' 오브젝트를 찾을 수 없습니다.");
            }
        }
    }
}
