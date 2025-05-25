using System.Collections.Generic;
using UnityEngine;

public class ItemSequenceController : MonoBehaviour
{
    [Header("Activation Distance")]
    [Tooltip("Trigger activation when player is within this distance")]  
    public float activationDistance = 1f;

    [Header("Items in sequence")]
    [Tooltip("Drag your arrows here in order (arrow1, arrow2, ...)")]
    public List<GameObject> items;

    [Header("Bobbing Settings")]
    [Tooltip("Vertical bob amplitude")]
    public float bobAmount = 0.3f;
    [Tooltip("Bob speed, cycles per second")]
    public float bobSpeed = 1f;

    private int currentIndex = 0;
    private Vector3[] originalPositions;
    private Transform playerTransform;

    void Start()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning("ItemSequenceController: 'Player' 태그의 오브젝트를 찾을 수 없습니다.");

        // Store original positions and deactivate all except the first
        originalPositions = new Vector3[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null)
            {
                originalPositions[i] = items[i].transform.position;
                items[i].SetActive(i == currentIndex);
            }
        }
    }

    void Update()
    {
        float t = Time.time;
        // Bob the active item
        if (currentIndex < items.Count)
        {
            GameObject item = items[currentIndex];
            if (item != null && item.activeSelf)
            {
                Vector3 pos = originalPositions[currentIndex];
                pos.y += Mathf.Sin(t * bobSpeed) * bobAmount;
                item.transform.position = pos;

                // Distance-based activation
                if (playerTransform != null)
                {
                    float dist = Vector3.Distance(playerTransform.position, pos);
                    if (dist <= activationDistance)
                    {
                        // Deactivate current and move to next
                        item.SetActive(false);
                        currentIndex++;
                        if (currentIndex < items.Count && items[currentIndex] != null)
                            items[currentIndex].SetActive(true);
                    }
                }
            }
        }
    }
}
