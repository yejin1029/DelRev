using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public float health;
    public float stamina;
    public int coinCount;
    public Vector3 playerPosition;
    public int currentDay;
    public List<string> inventoryItemNames = new List<string>();
}
