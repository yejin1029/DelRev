using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class CarTrigger : MonoBehaviour
{
    private bool isValid = false;

    IEnumerator Start()
    {
        yield return null;  // 한 프레임 대기 (다른 오브젝트들 Awake 대기)

        GameObject[] trailers = GameObject.FindGameObjectsWithTag("Car");

        foreach (var t in trailers)
        {
            if (t != gameObject && t.scene.name == "DontDestroyOnLoad")
            {
                Debug.LogWarning($"🛑 중복 트레일러 감지 → {gameObject.name} 제거");
                Destroy(gameObject);
                yield break;
            }
        }

        DontDestroyOnLoad(gameObject);
        isValid = true;
        Debug.Log("✅ 최초 트레일러 DontDestroyOnLoad 적용 완료");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isValid) return;

        if (!other.CompareTag("Item")) return;

        if (other.transform.parent == transform) return;

        other.transform.SetParent(transform);
        DontDestroyOnLoad(other.gameObject); // ✅ 자식 아이템도 보호
        Debug.Log($"📦 아이템 '{other.name}' → 트레일러 자식화 + DontDestroy");
    }
}
