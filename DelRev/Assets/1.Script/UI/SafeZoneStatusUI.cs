using UnityEngine;
using TMPro;

public class SafeZoneStatusUI : MonoBehaviour
{
    [Tooltip("세이프존에 있을 때만 보여줄 UI 오브젝트(텍스트/패널 등)")]
    public GameObject workPanel;

    private bool lastState = false;

    private void Start()
    {
        // 시작 시 초기 상태 반영
        UpdateUI(AreaGaugeController.PlayerInSafetyZone);
    }

    private void Update()
    {
        bool now = AreaGaugeController.PlayerInSafetyZone;

        if (now != lastState)
        {
            UpdateUI(now);
        }
    }

    private void UpdateUI(bool inSafeZone)
    {
        lastState = inSafeZone;

        if (workPanel != null)
            workPanel.SetActive(inSafeZone);
    }
}
