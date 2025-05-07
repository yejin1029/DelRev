// PlaneItemToCoin.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlaneItemToCoin : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("감지할 아이템 태그")]
    public string itemTag = "Item";

    private PlayerController playerController;

    private void Awake()
    {
        // Collider를 트리거로 설정
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // PlayerController 자동 참조 (Player 오브젝트에 "Player" 태그 필요)
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerController = playerObj.GetComponent<PlayerController>();
        else
            Debug.LogError("PlaneItemToCoin: Tag 'Player'가 붙은 오브젝트를 찾을 수 없습니다.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Item 태그를 가진 오브젝트만 처리
        if (!other.CompareTag(itemTag)) 
            return;

        // Item 컴포넌트에서 가격 가져와 플레이어에 추가
        var item = other.GetComponent<Item>();
        if (item != null && playerController != null)
        {
            playerController.AddCoins(item.itemPrice);
            Destroy(other.gameObject);
        }
    }
}
