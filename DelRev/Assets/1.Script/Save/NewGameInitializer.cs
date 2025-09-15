// NewGameInitializer.cs  (새 파일, 첫 플레이 씬 루트 오브젝트에 부착)
using UnityEngine;

public class NewGameInitializer : MonoBehaviour
{
    void Awake()
    {
        if (PlayerPrefs.GetInt("__NEW_GAME__", 0) != 1) return;

        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.health = 100f;
            player.stamina = player.maxStamina;
            player.coinCount = 0; // ★ 코인도 초기화
            player.isSpeedItemActive = false;

            // 스폰 포인트 이동(있을 경우)
            var cc = player.GetComponent<CharacterController>();
            var spawn = GameObject.FindWithTag("SpawnPoint")?.transform;
            if (spawn != null && cc != null)
            {
                bool was = cc.enabled; cc.enabled = false;
                player.transform.position = spawn.position;
                cc.enabled = was;
            }

            // UI 갱신
            // (PlayerController.ResetStats는 health/stamina만 초기화하므로 코인은 여기서 직접 반영)
            if (player.coinUI != null) player.coinUI.UpdateCoinText(player.coinCount);
        }

        var inv = FindObjectOfType<Inventory>();
        if (inv != null)
        {
            // 4칸 전부 비우기
            for (int i = 0; i < 4; i++) inv.SetItemAt(i, null);
        }

        // 맵/일자 초기화 (사용 중일 경우)
        try { if (MapTracker.Instance != null) MapTracker.Instance.currentDay = 0; } catch {}

        PlayerPrefs.SetInt("__NEW_GAME__", 0);
        PlayerPrefs.Save();

        Debug.Log("[NewGameInitializer] 새 게임 하드 리셋 완료");
    }
}
