// ContinueLoader.cs (새 스크립트 추가)
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueLoader : MonoBehaviour
{
    private static int pendingSlot = -1;

    public static void Begin(int slot)
    {
        // 중복 방지
        if (pendingSlot != -1) return;

        var go = new GameObject("[ContinueLoader]");
        DontDestroyOnLoad(go);
        var loader = go.AddComponent<ContinueLoader>();
        pendingSlot = slot;

        loader.StartCoroutine(loader.Run());
    }

    private IEnumerator Run()
    {
        string path = SaveLoadManager.GetSavePath(pendingSlot);
        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<SaveData>(json);

        // 1) 저장 당시 씬으로 이동
        var op = SceneManager.LoadSceneAsync(data.sceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        // 2) 한 프레임 대기(각 씬 오브젝트 Awake/Start/sceneLoaded 이후)
        yield return null;

        // 3) 대상 참조 찾아서 복원
        var player    = FindObjectOfType<PlayerController>();
        var inventory = FindObjectOfType<Inventory>();
        SaveLoadManager.LoadGame(player, inventory, pendingSlot);

        // 4) 커서 상태 보정(게임플레이)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // 정리
        pendingSlot = -1;
        Destroy(gameObject);
    }
}
