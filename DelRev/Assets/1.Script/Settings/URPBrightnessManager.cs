// URPBrightnessManager.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class URPBrightnessManager : MonoBehaviour
{
    public static URPBrightnessManager Instance { get; private set; }

    [Header("초기 EV 범위 권장: -3 ~ +3")]
    [SerializeField] private float defaultEV = 0f; // 0 = 기본 밝기
    [SerializeField] private string playerPrefsKey = "BrightnessEV";
    [SerializeField] private LayerMask volumeLayer = 0; // 비워두면 Default 사용

    private Volume volume;
    private VolumeProfile profile;
    private ColorAdjustments colorAdj;   // 대부분 버전에서 안전

    private const float MIN_EV = -3f;
    private const float MAX_EV =  3f;

    void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureVolumeAndProfile();

        // 저장값 불러와 적용
        float savedEV = PlayerPrefs.GetFloat(playerPrefsKey, defaultEV);
        SetEV(savedEV, save:false);

        // 씬 바뀔 때마다 혹시 프로파일/볼륨 레퍼런스가 바뀌었으면 다시 확보
        SceneManager.sceneLoaded += (_, __) => EnsureVolumeAndProfile();
    }

    private void EnsureVolumeAndProfile()
    {
        // 1) 이미 붙어있으면 사용
        if (volume == null)
        {
            volume = GetComponent<Volume>();
            if (volume == null)
            {
                volume = gameObject.AddComponent<Volume>();
                volume.isGlobal = true;
            }
        }
        volume.isGlobal = true;

        // 레이어 지정 (비워뒀으면 Default)
        if (volumeLayer == 0)
            gameObject.layer = LayerMask.NameToLayer("Default");
        else
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(volumeLayer.value, 2f));

        // 2) 프로파일 보장
        if (volume.profile == null)
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile = volume.profile;

        // 3) 필요한 Override 확보/추가
        if (!profile.TryGet(out colorAdj))
            colorAdj = profile.Add<ColorAdjustments>(true);
        colorAdj.postExposure.overrideState = true;
    }

    public float GetEV()
    {
        // ColorAdjustments 기준으로 반환
        if (colorAdj != null) return colorAdj.postExposure.value;
        return 0f;
    }

    public void SetEV(float ev, bool save = true)
    {
        ev = Mathf.Clamp(ev, MIN_EV, MAX_EV);

        if (colorAdj != null)
            colorAdj.postExposure.value = ev;
            
        if (save)
        {
            PlayerPrefs.SetFloat(playerPrefsKey, ev);
            PlayerPrefs.Save();
        }
    }
}
