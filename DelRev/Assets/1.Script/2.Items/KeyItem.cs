using UnityEngine;
using SuburbanHouse;

public class KeyItem : Item, IInventoryEffect
{
    [Tooltip("이 열쇠로 열 수 있는 문을 식별하는 ID")]
    public string doorID;

    // 패시브 효과 없음
    public void OnAdd(PlayerController player) { }
    public void OnRemove(PlayerController player) { }

    /// <summary>
    /// 맞는 문을 열고, 열쇠는 인벤토리에서 즉시 소멸
    /// </summary>
    public void UseOnDoor(KeyDoor door)
    {
        if (door == null) return;

        if (door.doorID == doorID)
        {
            door.UnlockAndOpen();

            var inv = Inventory.Instance;
            if (inv != null)
                inv.RemoveCurrentItemWithoutDrop(); // 드랍 없이 제거

            // 안전 차원에서 씬 상에서도 제거
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("이 문에는 맞지 않는 열쇠입니다.");
        }
    }
}
