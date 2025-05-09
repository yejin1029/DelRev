using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SkyboxTimeline : MonoBehaviour
{
    [Header("Skybox Settings")]
    public Material[] skyboxMaterials;
    public float totalDuration = 100f;

    private float interval => (skyboxMaterials.Length > 0) ? totalDuration / skyboxMaterials.Length : 1f;

    void OnValidate()
    {
        // 인스펙터에서 머티리얼 배열을 수정할 때 첫 번째 Skybox 적용 (편집 중 즉시 반영)
        if (skyboxMaterials != null && skyboxMaterials.Length > 0 && skyboxMaterials[0] != null)
        {
            RenderSettings.skybox = skyboxMaterials[0];
        }
    }

    void Update()
    {
        // 실행 중이 아닐 때는 아무것도 하지 않음
        if (!Application.isPlaying || skyboxMaterials == null || skyboxMaterials.Length == 0)
            return;

        float time = Time.time;

        int index;
        if (time >= totalDuration)
        {
            // 시간이 duration을 넘으면 마지막 Skybox로 고정
            index = skyboxMaterials.Length - 1;
        }
        else
        {
            // 현재 시간에 따른 Skybox 인덱스 계산
            index = Mathf.FloorToInt(time / interval);
            index = Mathf.Clamp(index, 0, skyboxMaterials.Length - 1);
        }

        if (skyboxMaterials[index] != null)
        {
            RenderSettings.skybox = skyboxMaterials[index];
        }
    }
}
