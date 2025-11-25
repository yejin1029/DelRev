using UnityEngine;
using System.Collections;

public class HealthItem : MonoBehaviour
{
    [Tooltip("회복시킬 체력 양")]
    public float healAmount = 20f;

    [Tooltip("아이템이 다시 생기기까지의 시간 (초)")]
    public float respawnTime = 5f;

    [Header("Audio")]
    [Tooltip("회복 아이템을 먹었을 때 재생할 사운드")]
    public AudioClip pickupSfx;
    [Range(0f, 1f)] public float pickupVolume = 1f;

    private MeshRenderer meshRenderer;
    private Collider itemCollider;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        itemCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌했을 때만
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.health += healAmount;
                player.health = Mathf.Clamp(player.health, 0f, 100f);
                player.SendMessage("UpdateHealthUI", SendMessageOptions.DontRequireReceiver);
            }

            // 🔊 먹었을 때 사운드 재생
            if (pickupSfx != null)
                AudioSource.PlayClipAtPoint(pickupSfx, transform.position, pickupVolume);

            // 아이템 숨기기 + 리스폰 대기
            StartCoroutine(RespawnItem());
        }
    }

    private IEnumerator RespawnItem()
    {
        meshRenderer.enabled = false;
        itemCollider.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        meshRenderer.enabled = true;
        itemCollider.enabled = true;
    }
}
