using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI coinText;

    private int currentDay = 0;

    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();
    }

    private void Start()
    {
        // 시작 시 코인 한 번 맞춰두기
        RefreshCoinText();
    }

    // 매 프레임 제일 마지막에 내가 최종값으로 덮어쓰기 (다른 스크립트가 써도 이게 이김)
    private void LateUpdate()
    {
        RefreshCoinText();
    }

    private void RefreshCoinText()
    {
        if (coinText == null)
            return;

        int currentCoins = 0;
        if (player != null)
            currentCoins = player.coinCount;

        int required = 0;
        var tracker = MapTracker.Instance;

        if (tracker != null &&
            tracker.checkDays != null &&
            tracker.coinRequirements != null &&
            tracker.checkDays.Count > 0 &&
            tracker.coinRequirements.Count > 0)
        {
            var days = tracker.checkDays;
            var reqs = tracker.coinRequirements;

            for (int i = 0; i < days.Count; i++)
            {
                if (currentDay < days[i])
                {
                    int idx = Mathf.Clamp(i, 0, reqs.Count - 1);
                    required = reqs[idx];
                    break;
                }
            }

            if (required == 0)
                required = reqs[reqs.Count - 1];
        }

        coinText.text = $"{currentCoins}/{required} Coins";
    }

    public void UpdateCoinText(int coinCount)
    {
        // PlayerController에서 호출하지만,
        // 실제 갱신은 RefreshCoinText에서 처리
        RefreshCoinText();
    }
}
