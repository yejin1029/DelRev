using UnityEngine;

[DisallowMultipleComponent]
public class FlameProjectile : MonoBehaviour
{
    float speed;
    float lifeDistance;
    float traveled;
    float dps;
    float radius;

    CapsuleCollider cap;
    Rigidbody rb;

    [Header("🔥 Visual Effect")]
    [Tooltip("이 발사체의 비주얼 이펙트 프리팹 (예: VFX_Fire_01_Big)")]
    public GameObject fireVFXPrefab;

    private GameObject fireVFXInstance;

    public void Initialize(float speed, float lifeDistance, float radius, float dps)
    {
        this.speed = speed;
        this.lifeDistance = Mathf.Max(0.1f, lifeDistance);
        this.radius = Mathf.Max(0.05f, radius);
        this.dps = Mathf.Max(0f, dps);

        // 🔹 1) 콜라이더 (Trigger)
        cap = gameObject.AddComponent<CapsuleCollider>();
        cap.isTrigger = true;
        cap.direction = 2; // Z축
        cap.center = Vector3.zero;
        cap.radius = this.radius;
        cap.height = this.radius * 2f;

        // 🔹 2) Rigidbody (Trigger 안정성)
        rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // 🔹 3) 불 프리팹 생성
        if (fireVFXPrefab != null)
        {
            fireVFXInstance = Instantiate(fireVFXPrefab, transform.position, transform.rotation, transform);
            fireVFXInstance.transform.localScale = Vector3.one * (this.radius * 3f);
        }
        else
        {
            Debug.LogWarning("[FlameProjectile] fireVFXPrefab이 지정되지 않음 — 기본 Sphere로 표시합니다.");
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform, false);
            sphere.transform.localScale = Vector3.one * (this.radius * 2f);
            var sr = sphere.GetComponent<SphereCollider>();
            if (sr) Destroy(sr);
        }

    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position += transform.forward * step;
        traveled += step;

        if (traveled >= lifeDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null && dps > 0f)
        {
            player.TakeDamage(dps * Time.deltaTime);
        }
    }

    void OnDestroy()
    {
        if (fireVFXInstance != null)
            Destroy(fireVFXInstance);
    }
}
