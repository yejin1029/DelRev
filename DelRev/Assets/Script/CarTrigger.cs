// TrailerTrigger.cs
using UnityEngine;
using UnityEngine.SceneManagement;  // ← 추가

[RequireComponent(typeof(Collider))]
public class TrailerTrigger : MonoBehaviour
{
    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            other.transform.SetParent(transform);
            Debug.Log($"{other.name}: Parent set to {name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            // 1) 부모 관계 해제
            other.transform.SetParent(null);
            // 2) 다시 '현재 활성 씬'으로 옮겨서
            //    씬 언로드 시에만 파괴되게 만들기
            SceneManager.MoveGameObjectToScene(
                other.gameObject,
                SceneManager.GetActiveScene()
            );

            Debug.Log($"{other.name}: Unparented and moved back to scene '{SceneManager.GetActiveScene().name}'");
        }
    }
}
