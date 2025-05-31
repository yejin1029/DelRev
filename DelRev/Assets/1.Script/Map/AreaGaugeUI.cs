using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AreaGaugeUI : MonoBehaviour
{
    public static AreaGaugeUI Instance { get; private set; }

    [Header("Gauge UI")]
    public Image gaugeFillImage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetGauge(); // ✅ 씬 이동 시 게이지 자동 초기화
    }

    public void UpdateGaugeUI(float gaugePercent)
    {
        if (gaugeFillImage != null)
            gaugeFillImage.fillAmount = Mathf.Clamp01(gaugePercent / 100f);
    }

    public void ResetGauge()
    {
        UpdateGaugeUI(0f); // ✅ 게이지를 0으로 설정
    }
}
