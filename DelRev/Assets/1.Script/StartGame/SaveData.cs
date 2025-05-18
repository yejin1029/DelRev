using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
  public float[] playerPosition;
  public float health;
  public float stamina;
  public int coinCount;
  public List<string> inventoryItemNames;
  public int currentInventoryIndex;
}
