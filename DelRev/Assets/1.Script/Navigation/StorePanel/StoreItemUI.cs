using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public Image outlineImage;

    public string itemName;
    public int price;

    void Start()
    {
        itemNameText.text = itemName;
        priceText.text = $": {price} Coins";

        buyButton.onClick.AddListener(OnBuy);
    }

    public void SetSelected(bool isSelected)
    {
        if (outlineImage != null)
            outlineImage.color = isSelected ? Color.yellow : Color.clear;
    }

    public void OnBuy()
    {
        var player = PlayerController.Instance;
        var inventory = Inventory.Instance;

        if (player == null || inventory == null) return;

        if (player.coinCount < price)
        {
            Debug.Log("코인이 부족합니다!");
            WarningUI.Instance?.ShowWarning("코인이 부족합니다!");
            return;
        }

        var items = inventory.GetInventoryItems();
        bool hasSpace = items.Exists(item => item == null);
        if (!hasSpace)
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
            WarningUI.Instance?.ShowWarning("인벤토리가 가득 찼습니다!");
            return;
        }

        // 코인 차감
        player.SubtractCoins(price);

        // 아이템 인스턴스 생성 및 인벤토리에 추가
        var itemPrefab = Resources.Load<GameObject>($"Items/{itemName}");
        if (itemPrefab != null)
        {
            GameObject newItem = GameObject.Instantiate(itemPrefab);
            var itemComponent = newItem.GetComponent<Item>();
            if (itemComponent != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] == null)
                    {
                        inventory.SetItemAt(i, itemComponent);
                        break;
                    }
                }
            }
        }

        Debug.Log($"{itemName} 구매 완료!");
    }
}

