// DisplayBoot.cs
using UnityEngine;

public static class DisplayBoot
{
    private const string KEY_MODE = "FullScreenMode"; // int로 저장
    private const string KEY_WIN_W = "WindowedWidth";
    private const string KEY_WIN_H = "WindowedHeight";

    // 기본값: 경계 없는 전체화면(권장) / 창모드 기본 크기: 1280x720
    private const FullScreenMode DEFAULT_MODE = FullScreenMode.FullScreenWindow;
    private const int DEFAULT_WIN_W = 1280;
    private const int DEFAULT_WIN_H = 720;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplySavedDisplay()
    {
        var savedMode = (FullScreenMode)PlayerPrefs.GetInt(KEY_MODE, (int)DEFAULT_MODE);
        int w = PlayerPrefs.GetInt(KEY_WIN_W, DEFAULT_WIN_W);
        int h = PlayerPrefs.GetInt(KEY_WIN_H, DEFAULT_WIN_H);

        ApplyDisplay(savedMode, w, h);
    }

    public static void ApplyDisplay(FullScreenMode mode, int windowedWidth, int windowedHeight)
    {
        // 창모드일 땐 원하는 창 크기로, 나머지는 모드만 적용
        if (mode == FullScreenMode.Windowed)
        {
            Screen.SetResolution(windowedWidth, windowedHeight, FullScreenMode.Windowed);
        }
        else
        {
            Screen.fullScreenMode = mode; // FullScreenWindow, ExclusiveFullScreen, MaximizedWindow 등
        }
    }

    public static void SaveWindowedSizeIfNeeded()
    {
        if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            PlayerPrefs.SetInt(KEY_WIN_W, Screen.width);
            PlayerPrefs.SetInt(KEY_WIN_H, Screen.height);
            PlayerPrefs.Save();
        }
    }

    public static void SaveMode(FullScreenMode mode)
    {
        PlayerPrefs.SetInt(KEY_MODE, (int)mode);
        PlayerPrefs.Save();
    }
}
