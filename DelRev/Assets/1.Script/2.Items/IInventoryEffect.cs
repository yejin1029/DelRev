public interface IInventoryEffect
{
    // 인벤토리에 집어넣을 때
    void OnAdd(PlayerController player);

    // 인벤토리에서 뺄 때
    void OnRemove(PlayerController player);
}
