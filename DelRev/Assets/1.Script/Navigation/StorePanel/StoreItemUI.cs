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
            outlineImage.gameObject.SetActive(isSelected);
    }

    public void OnBuy()
    {
        var player = PlayerController.Instance;
        var inventory = Inventory.Instance;

        if (player == null || inventory == null) return;

        if (player.coinCount < price)
        {
            WarningUI.Instance?.ShowWarning("코인이 부족합니다!");
            return;
        }

        var items = inventory.GetInventoryItems();
        bool hasSpace = items.Exists(item => item == null);
        if (!hasSpace)
        {
            WarningUI.Instance?.ShowWarning("인벤토리를 비워주세요!");
            return;
        }

        // 코인 차감
        player.SubtractCoins(price);

        // 아이템 프리팹 불러오기
        GameObject prefab = Resources.Load<GameObject>($"StoreItems/{itemName}");
        if (prefab == null)
        {
            Debug.LogError($"프리팹 Items/{itemName} 을(를) 찾을 수 없습니다.");
            return;
        }

        // 실제 인벤토리용 아이템 생성
        GameObject newItem = Instantiate(prefab);
        Item itemComponent = newItem.GetComponent<Item>();

        if (itemComponent == null)
        {
            Debug.LogError("프리팹에 Item 컴포넌트가 없습니다.");
            return;
        }

        // 빈 슬롯에 아이템 넣기
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                inventory.SetItemAt(i, itemComponent);
                break;
            }
        }

        Debug.Log($"{itemName} 구매 완료 및 인벤토리 등록!");
    }
}

