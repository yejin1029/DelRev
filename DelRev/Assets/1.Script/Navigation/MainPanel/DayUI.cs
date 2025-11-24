using UnityEngine;
using TMPro;

public class DayUI : MonoBehaviour
{
    public TextMeshProUGUI dayText;

    void Update()
    {
        if (dayText == null) return;

        if (DayManager.Instance == null)
        {
            dayText.text = "Day -";
            return;
        }

        int day = DayManager.Instance.CurrentDay;
        dayText.text = $"Day {day}";
    }
}
