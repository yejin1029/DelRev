using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public Image outlineImage;

    [Header("Item Info")]
    public string itemName;
    public int price;

    [Header("Audio")]
    public AudioClip purchaseSfx;   // 성공 사운드
    public AudioClip failSfx;       // 실패 사운드
    public float sfxVolume = 1f;

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

        if (player == null || inventory == null)
        {
            Debug.LogError("[StoreItemUI] PlayerController 또는 Inventory 인스턴스가 없습니다!");
            PlayFailSound("필수 시스템 없음");
            return;
        }

        // AudioListener 디버그
        if (Camera.main == null || Camera.main.GetComponent<AudioListener>() == null)
            Debug.LogWarning("[StoreItemUI] 메인 카메라 또는 AudioListener가 없습니다!");

        // ==== 1) 코인 부족 ====
        if (player.coinCount < price)
        {
            WarningUI.Instance?.ShowWarning("코인이 부족합니다!");
            PlayFailSound("코인 부족");
            return;
        }

        // ==== 2) 인벤토리 공간 부족 ====
        var items = inventory.GetInventoryItems();
        bool hasSpace = items.Exists(item => item == null);

        if (!hasSpace)
        {
            WarningUI.Instance?.ShowWarning("인벤토리를 비워주세요!");
            PlayFailSound("인벤토리 부족");
            return;
        }

        // ==== 성공 처리 ====
        player.SubtractCoins(price);

        GameObject prefab = Resources.Load<GameObject>($"StoreItems/{itemName}");
        if (prefab == null)
        {
            Debug.LogError($"[StoreItemUI] 프리팹 'StoreItems/{itemName}' 을(를) 찾을 수 없습니다!");
            PlayFailSound("프리팹 없음");
            return;
        }

        GameObject newItem = Instantiate(prefab);
        Item itemComponent = newItem.GetComponent<Item>();

        if (itemComponent == null)
        {
            Debug.LogError("[StoreItemUI] 프리팹에 Item 컴포넌트가 없습니다!");
            PlayFailSound("Item 없음");
            return;
        }

        // 슬롯에 넣기
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                inventory.SetItemAt(i, itemComponent);
                break;
            }
        }

        Debug.Log($"[StoreItemUI] {itemName} 구매 성공!");
        PlayPurchaseSound();
    }

    // ============================================================
    // 🔊 사운드 재생 함수들
    // ============================================================

    void PlayPurchaseSound()
    {
        if (purchaseSfx == null)
        {
            Debug.LogWarning("[StoreItemUI] purchaseSfx가 비어있음 → 소리 재생 불가");
            return;
        }

        Debug.Log("[StoreItemUI] 구매 성공 사운드 재생!");

        Vector3 pos = Camera.main != null ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(purchaseSfx, pos, sfxVolume);
    }

    void PlayFailSound(string reason)
    {
        if (failSfx == null)
        {
            Debug.LogWarning($"[StoreItemUI] 실패 사운드 없음! ({reason})");
            return;
        }

        Debug.Log($"[StoreItemUI] 구매 실패 사운드 재생! 이유: {reason}");

        Vector3 pos = Camera.main != null ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(failSfx, pos, sfxVolume);
    }
}
