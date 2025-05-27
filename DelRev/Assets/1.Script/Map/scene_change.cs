using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    [Header("씬 이름 설정")]
    public string sceneMoveName = "Scene_move";
    public string playerTestName = "PlayerTest";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            ChangeScene(sceneMoveName);
        if (Input.GetKeyDown(KeyCode.O))
            ChangeScene(playerTestName);
    }

    public void ChangeScene(string targetScene)
    {
        StartCoroutine(DelayedSceneChange(targetScene));
    }

    IEnumerator DelayedSceneChange(string targetScene)
    {
        CleanUpLooseItems();
        yield return null;
        SceneManager.LoadScene(targetScene);
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
}
