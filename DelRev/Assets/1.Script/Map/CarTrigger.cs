using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class CarTrigger : MonoBehaviour
{
    void Awake()
    {
        // 트레일러(차량) 자체를 영속 오브젝트로 만들어
        // 씬 전환 시 파괴되지 않게 함
        DontDestroyOnLoad(gameObject);

        // SphereCollider를 트리거로 설정하고 반경 1로 지정
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius    = 1f;
    }

    void OnTriggerEnter(Collider other)
    {
        // Item 태그만 처리
        if (!other.CompareTag("Item")) return;

        // 이미 자식이면 무시
        if (other.transform.parent == transform) return;

        // CarTrigger의 자식으로 삼아 함께 이동·영속화
        other.transform.SetParent(transform);
    }
}
