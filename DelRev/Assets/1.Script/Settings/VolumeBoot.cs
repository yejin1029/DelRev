// VolumeBoot.cs
using UnityEngine;

public static class VolumeBoot
{
    private const string KEY = "MasterVolume";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplySavedVolume()
    {
        float v = PlayerPrefs.GetFloat(KEY, 1f); // 기본값 1
        AudioListener.volume = Mathf.Clamp01(v);
    }
}
