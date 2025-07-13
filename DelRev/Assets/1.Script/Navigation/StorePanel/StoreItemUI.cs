using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    public string itemName;
    public int price;

    void Start()
    {
        itemNameText.text = itemName;
        priceText.text = $"{price} Coins";

        buyButton.onClick.AddListener(OnBuy);
    }

    void OnBuy()
    {
        if (PlayerController.Instance.coinCount >= price)
        {
            PlayerController.Instance.coinCount -= price;
            Debug.Log($"{itemName} 구매 완료!");
            // 아이템 지급 로직 추가 가능
        }
        else
        {
            Debug.Log("코인이 부족합니다!");
        }
    }
}

