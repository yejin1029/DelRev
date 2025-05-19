using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
  public float[] playerPosition;
  public float health;
  public float stamina;
  public int coinCount;

  public List<string> inventoryItemNames;
  public int currentInventoryIndex;

  public float[] cameraLocalPosition;
  public float[] cameraLocalRotation;
}
