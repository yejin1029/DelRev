using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
  public static Item GetItemByName(string itemName)
  {
    Item prefab = Resources.Load<Item>("Items/" + itemName);
    if (prefab == null)
    {
      Debug.LogWarning($"ItemDatabase: '{itemName}' 프리팹을 찾을 수 없습니다.");
    }
    return prefab;
  }
}