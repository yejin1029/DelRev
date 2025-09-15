using UnityEngine;

public static class GlobalState
{
    // DontDestroyOnLoad 씬의 루트 오브젝트를 전부 제거
    public static void KillAllDontDestroyOnLoad()
    {
        var marker = new GameObject("__DDOL_MARKER__");
        Object.DontDestroyOnLoad(marker);
        var ddolScene = marker.scene;

        foreach (var root in ddolScene.GetRootGameObjects())
        {
            if (root == marker) continue;
            Object.Destroy(root);
        }
        Object.Destroy(marker);
    }
}
