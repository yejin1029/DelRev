// URPBrightnessSlider.cs
using UnityEngine;
using UnityEngine.UI;

public class URPBrightnessSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;

    void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();

        // 저장된 EV로 초기화
        if (URPBrightnessManager.Instance != null)
            slider.value = URPBrightnessManager.Instance.GetEV();

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float ev)
    {
        if (URPBrightnessManager.Instance != null)
            URPBrightnessManager.Instance.SetEV(ev, save:true);
    }
}
