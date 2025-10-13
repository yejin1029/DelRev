// WindowModeToggle.cs
using UnityEngine;
using UnityEngine.UI;

public class WindowModeToggle : MonoBehaviour
{
    [SerializeField] private Toggle toggle; // On = Windowed, Off = FullScreenWindow
    [SerializeField] private int windowedWidth = 1280;
    [SerializeField] private int windowedHeight = 720;

    private const string KEY_MODE = "FullScreenMode";

    void Awake()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();

        // 현재 저장된 모드를 읽어와 토글 초기화
        var savedMode = (FullScreenMode)PlayerPrefs.GetInt(KEY_MODE, (int)FullScreenMode.FullScreenWindow);
        toggle.isOn = (savedMode == FullScreenMode.Windowed);

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isWindowed)
    {
        var mode = isWindowed ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;

        if (isWindowed)
        {
            // 창모드로 전환 + 크기 적용
            DisplayBoot.ApplyDisplay(mode, windowedWidth, windowedHeight);
        }
        else
        {
            // 경계 없는 전체화면으로 전환
            DisplayBoot.ApplyDisplay(mode, windowedWidth, windowedHeight);
        }

        DisplayBoot.SaveMode(mode);
    }

    // 선택사항: Company 씬에서 떠날 때 현재 창 크기를 저장(창모드인 경우)
    void OnDisable()
    {
        DisplayBoot.SaveWindowedSizeIfNeeded();
    }
}
