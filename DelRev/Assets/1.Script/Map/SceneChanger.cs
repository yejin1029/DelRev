using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    public static bool isChanging = false;

    [Header("테스트용")]
    public string sceneMoveName = "FamilyHouse";
    public string playerTestName = "Company";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) ChangeScene(sceneMoveName);
        if (Input.GetKeyDown(KeyCode.O)) ChangeScene(playerTestName);
    }

    public void ChangeScene(string targetScene)
    {
        if (isChanging)
        {
            Debug.LogWarning("[SceneChanger] 이미 전환 중입니다.");
            return;
        }
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("[SceneChanger] 타겟 씬 이름이 비어 있습니다.");
            return;
        }
        StartCoroutine(GoWithLoading(targetScene));
    }

    private IEnumerator GoWithLoading(string targetScene)
    {
        isChanging = true;

        // 필요 시 정리
        CleanUpLooseItems();

        // 타임스케일 0일 수 있으니 Realtime로 잠깐 여유
        yield return new WaitForSecondsRealtime(0.05f);

        // ⬇️ 핵심: 로딩씬으로 들어간 뒤, 거기서 targetScene을 비동기로 로드
        SceneLoader.Load(targetScene);

        // 이 오브젝트가 DDOL이면 다음 씬 로드 완료 후 초기화
        yield return null;
        isChanging = false;
    }

    public void CleanUpLooseItems()
    {
        GameObject[] allItems = GameObject.FindGameObjectsWithTag("Item");
        int removed = 0;

        foreach (GameObject item in allItems)
        {
            Transform parent = item.transform.parent;

            bool isInsideTrailer =
                parent != null &&
                (parent.CompareTag("Car") || parent.GetComponentInParent<CarTrigger>() != null);

            if (!isInsideTrailer)
            {
                Destroy(item);
                removed++;
            }
        }
        Debug.Log($"🧹 트레일러 외부 아이템 {removed}개 제거 완료");
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene s, LoadSceneMode m) => isChanging = false;
}
