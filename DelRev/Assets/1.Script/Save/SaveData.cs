using System;
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
    public string sceneName;
    public List<string> inventoryItemNames = new List<string>(); // 크기 4 사용

    // --- Trailer 내부 아이템 목록 ---
    [Serializable]
    public class TrailerItemData
    {
        public string key;                // itemName 또는 프리팹명(매칭 키)
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    public List<TrailerItemData> trailerItems = new List<TrailerItemData>();
}