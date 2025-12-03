using UnityEngine;
using UnityEngine.Video;

public class IntroVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "Company";

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // 영상 끝날 때 호출될 콜백 등록
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }

    private void Update()
    {
        // ESC 키 누르면 스킵
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Skip();
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        // 영상 끝나면 로딩씬 거쳐서 Company로
        SceneLoader.Load(nextSceneName);
    }

    // 스킵 버튼에 연결할 함수 (선택)
    public void Skip()
    {
        OnVideoEnd(videoPlayer);
    }
}
