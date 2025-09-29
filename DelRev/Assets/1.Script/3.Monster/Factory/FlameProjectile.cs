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

    // 즉석으로 "보이는 구체 + 꼬리"를 만들어서 이펙트 없이도 확인 가능
    public void Initialize(float speed, float lifeDistance, float radius, float dps)
    {
        this.speed = speed;
        this.lifeDistance = Mathf.Max(0.1f, lifeDistance);
        this.radius = Mathf.Max(0.05f, radius);
        this.dps = Mathf.Max(0f, dps);

        // 1) 콜라이더(Trigger)
        cap = gameObject.AddComponent<CapsuleCollider>();
        cap.isTrigger = true;
        cap.direction = 2; // Z축
        cap.center = Vector3.zero;
        cap.radius = this.radius;
        cap.height = this.radius * 2f;

        // 2) Rigidbody(Trigger 이벤트 안정성을 위해)
        rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // 3) 가시화용 구체 MeshRenderer(주황색)
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "VisSphere";
        sphere.transform.SetParent(transform, false);
        sphere.transform.localScale = Vector3.one * (this.radius * 2f);
        var sr = sphere.GetComponent<SphereCollider>();
        if (sr) Destroy(sr); // 불필요한 콜라이더 제거
        var mr = sphere.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.material = new Material(Shader.Find("Standard"));
            mr.material.SetColor("_Color", new Color(1f, 0.45f, 0.05f)); // 주황
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        // 4) 꼬리(트레일) — 자동으로 기본 색/길이
        var trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.12f;
        trail.startWidth = this.radius * 2f;
        trail.endWidth = 0.01f;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
        // 기본 머티리얼 자동 생성(색을 따로 안 주면 흰색->주황 구체 색과 합쳐서 충분히 보임)

        // 5) 수명 관리: 최대 거리 도달 시 파괴
        // (Update에서 traveled로 체크)
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
}
