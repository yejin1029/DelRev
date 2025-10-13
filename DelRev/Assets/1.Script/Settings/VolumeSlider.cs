// VolumeSlider.cs
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private const string KEY = "MasterVolume";

    void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();
        slider.value = PlayerPrefs.GetFloat(KEY, 0.7f);
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY, AudioListener.volume);
        PlayerPrefs.Save();
    }
}
