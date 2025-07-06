using UnityEngine;

// 인벤토리에 들어오고 나갈 때 자동으로 OnAdd/OnRemove가 호출됩니다.
public class SpeedBoostItem : Item, IInventoryEffect
{
    [Tooltip("이 아이템을 인벤토리에 넣으면 추가되는 속도 배율 (예: 0.2 = +20%)")]
    public float speedBonus = 0.2f;

    // 인벤토리에 담겼을 때 호출
    public void OnAdd(PlayerController player)
    {
        if (player == null) return;
        player.inventorySpeedMultiplier *= (1 + speedBonus);
    }

    // 인벤토리에서 제거(버리기)될 때 호출
    public void OnRemove(PlayerController player)
    {
        if (player == null) return;
        player.inventorySpeedMultiplier /= (1 + speedBonus);
    }
}
