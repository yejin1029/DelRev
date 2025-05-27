using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class CarTrigger : MonoBehaviour
{
    IEnumerator Awake()
    {
        yield return null; // 한 프레임 대기: 기존 DontDestroy 트레일러 먼저 인식되도록

        GameObject[] trailers = GameObject.FindGameObjectsWithTag("Car");

        foreach (var t in trailers)
        {
            if (t != gameObject && t.scene.name == "DontDestroyOnLoad")
            {
                Debug.LogWarning($"🛑 중복 트레일러 감지 → {gameObject.name} 제거");
                Destroy(gameObject); // 나는 씬에 새로 로드된 트레일러
                yield break;
            }
        }

        DontDestroyOnLoad(gameObject);
        Debug.Log("✅ 최초 트레일러 DontDestroyOnLoad 적용 완료");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Item")) return;

        if (other.transform.parent == transform) return;

        other.transform.SetParent(transform);
        DontDestroyOnLoad(other.gameObject); // ✅ 자식 아이템도 영속화
        Debug.Log($"📦 아이템 '{other.name}' → 트레일러 자식화 + DontDestroy");
    }
}
