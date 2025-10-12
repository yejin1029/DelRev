using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 로딩씬에 붙여서 사용.
/// ProgressBar(Image.fillAmount) 업데이트 + 완료시 자동 활성화.
/// </summary>
public class LoadingSceneController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("FillAmount로 채울 프로그레스 바 이미지")]
    public Image progressBar;          // 이미 만들어둔 ProgressBar(Image)

    [Tooltip("0.9f → 1.0f 보간 속도(연출용)")]
    public float smoothing = 0.75f;

    private void Start()
    {
        StartCoroutine(LoadProcess());
    }

    private IEnumerator LoadProcess()
    {
        string next = SceneLoader.NextSceneName;
        if (string.IsNullOrEmpty(next))
        {
            Debug.LogError("[LoadingScene] NextSceneName이 비어 있습니다.");
            yield break;
        }

        // 실제 타겟 씬 비동기 로드(활성화는 나중에)
        AsyncOperation op = SceneManager.LoadSceneAsync(next, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        float fakeFill = 0f;

        while (!op.isDone)
        {
            // Unity의 op.progress는 0~0.9 까지 진행 → 0.9가 로드 완료 의미
            float real = Mathf.Clamp01(op.progress / 0.9f);

            // 0.9 이후 1.0까지는 연출용 보간
            fakeFill = Mathf.MoveTowards(fakeFill, real, smoothing * Time.unscaledDeltaTime);

            if (progressBar != null)
                progressBar.fillAmount = fakeFill;

            // 다 찼으면 씬 활성화
            if (fakeFill >= 1f - 0.0001f && op.progress >= 0.9f)
            {
                op.allowSceneActivation = true;
            }

            yield return null; // 매 프레임
        }
    }
}
