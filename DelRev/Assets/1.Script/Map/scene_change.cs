// SceneChanger.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("씬 이름 설정")]
    public string sceneMoveName  = "Scene_move";
    public string playerTestName = "PlayerTest";

    [Header("트레일러 주변 반경(보호 영역)")]
    public float keepRadius = 5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            ChangeScene(sceneMoveName);
        if (Input.GetKeyDown(KeyCode.O))
            ChangeScene(playerTestName);
    }

    public void ChangeScene(string targetScene)
    {
        CleanUpLooseItems();
        SceneManager.LoadScene(targetScene);
    }

    public void CleanUpLooseItems()
    {
        // 1) 씬 안의 현재 트레일러 찾기
        var trailerObj = GameObject.FindGameObjectWithTag("Car");
        if (trailerObj == null)
        {
            Debug.LogWarning("CleanUpLooseItems: 트레일러(Car) 오브젝트를 찾을 수 없습니다.");
            return;
        }
        Vector3 trailerPos = trailerObj.transform.position;
    }
}
