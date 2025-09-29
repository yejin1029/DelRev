using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections.Generic;

public class AudioSettingsController : MonoBehaviour
{
    [Header("Mixer & Exposed Param Names")]
    public AudioMixer mixer;
    public string masterParam = "MasterVol";
    public string musicParam  = "MusicVol";
    public string sfxParam    = "SFXVol";

    [Header("UI")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle masterMuteToggle;
    public Toggle musicMuteToggle;
    public Toggle sfxMuteToggle;

    const float MinDb = -80f;   // 사실상 무음
    const float DefaultLinear = 0.8f;

    // 뮤트 해제 시 복원할 마지막 값 기억
    private readonly Dictionary<string, float> lastLinear = new();

    void Awake()
    {
        InitControl(masterSlider, masterMuteToggle, masterParam, "vol_master");
        InitControl(musicSlider,  musicMuteToggle,  musicParam,  "vol_music");
        InitControl(sfxSlider,    sfxMuteToggle,    sfxParam,    "vol_sfx");
    }

    void InitControl(Slider slider, Toggle toggle, string param, string prefKey)
    {
        // 로드(없으면 기본 0.8)
        float saved = PlayerPrefs.GetFloat(prefKey, DefaultLinear);
        if (slider != null)
        {
            slider.SetValueWithoutNotify(saved);
            slider.onValueChanged.AddListener(v =>
            {
                ApplyVolume(param, v);
                lastLinear[param] = v;
                PlayerPrefs.SetFloat(prefKey, v);
                PlayerPrefs.Save();
                // 뮤트 토글과 동기화(0이면 자동으로 체크)
                if (toggle != null)
                    toggle.SetIsOnWithoutNotify(v <= 0.0001f);
            });
        }
        ApplyVolume(param, saved);
        lastLinear[param] = saved;

        if (toggle != null)
        {
            // 저장된 값이 거의 0이면 뮤트 체크
            toggle.SetIsOnWithoutNotify(saved <= 0.0001f);
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn) // 뮤트
                {
                    mixer.SetFloat(param, MinDb);
                }
                else // 뮤트 해제 → 마지막 값 복원
                {
                    float lin = Mathf.Max(lastLinear[param], 0.0001f);
                    ApplyVolume(param, lin);
                    if (slider != null) slider.SetValueWithoutNotify(lin);
                }
            });
        }
    }

    // 0~1(선형) → 데시벨(-80~0) 변환하여 믹서에 적용
    void ApplyVolume(string param, float linear01)
    {
        float lin = Mathf.Clamp(linear01, 0.0001f, 1f);      // 0 로그 방지
        float dB = Mathf.Log10(lin) * 20f;                   // 표준 변환식
        mixer.SetFloat(param, Mathf.Max(dB, MinDb));         // 하한 클램프
    }
}
