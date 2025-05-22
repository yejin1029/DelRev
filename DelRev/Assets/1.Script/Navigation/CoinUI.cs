using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinUI : MonoBehaviour
{
  public PlayerController player;
  public TextMeshProUGUI coinText;

  void Start()
  {
    if (player == null)
      player = FindObjectOfType<PlayerController>();

    UpdateCoinText(player.coinCount);
  }

  public void UpdateCoinText(int coinCount)
  {
    coinText.text = $"{coinCount} Coins";
  }
}