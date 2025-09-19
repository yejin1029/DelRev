using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerDamage : MonoBehaviour
{
    [Tooltip("초당 데미지 (WeldingRobot.damagePerSecond와 동일하게 맞추세요)")]
    public float damagePerSecond = 15f;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        // 플레이어에만 데미지
        var player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
