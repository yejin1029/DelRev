using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI dayText;

    void Start()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        UpdateCoinText(player.coinCount);
    }

void Update()
{
    if (MapTracker.Instance != null && player != null)
    {
        int currentCoins = MapTracker.Instance.totalCoinCount;
        int day = MapTracker.Instance.currentDay;

        if (dayText != null)
            dayText.text = $"Day {day}";

        int required = 0;

        // 가장 가까운 '미래' 요구일 찾기
        for (int i = 0; i < MapTracker.Instance.checkDays.Count; i++)
        {
            if (day < MapTracker.Instance.checkDays[i])
            {
                required = MapTracker.Instance.coinRequirements[i];
                break;
            }
        }

        coinText.text = $"{currentCoins}/{required} Coins";
    }
}

    public void UpdateCoinText(int coinCount)
    {
        // 이 메서드는 필요 없을 수 있지만, 남겨둬도 무방합니다.
    }
}
