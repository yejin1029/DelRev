using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class RedOverlay : MonoBehaviour
{
    [Range(0,1)] public float alpha = 0.5f;
    private Image _overlay;

    void Awake()
    {
        // Canvas 컴포넌트 확인
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // 풀스크린 Image 생성
        var go = new GameObject("RedOverlayImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _overlay = go.GetComponent<Image>();
        _overlay.color = new Color(1f, 0f, 0f, alpha);
        _overlay.raycastTarget = false;  // 클릭 등을 막고 싶지 않다면 꺼두기
    }

    /// <summary>
    /// 원하는 시점에 투명도나 표시 여부를 바꾸고 싶으면 호출
    /// </summary>
    public void SetAlpha(float a)
    {
        alpha = Mathf.Clamp01(a);
        if (_overlay != null)
            _overlay.color = new Color(1f, 0f, 0f, alpha);
    }

    public void Show(bool on)
    {
        if (_overlay != null)
            _overlay.enabled = on;
    }
}
