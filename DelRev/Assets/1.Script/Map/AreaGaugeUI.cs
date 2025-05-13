using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

  public void UpdateGaugeUI(float gaugePercent)
  {
    if (gaugeFillImage != null)
      gaugeFillImage.fillAmount = Mathf.Clamp01(gaugePercent / 100f);
  }
}
