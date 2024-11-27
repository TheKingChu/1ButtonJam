using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStats: MonoBehaviour
{
    public TMP_Text coinsText;

    private void Start()
    {
        UpdateCoinUI();
    }

    private void UpdateCoinUI()
    {
        int coins = GameManager.Instance.playerCoins;
        if (coinsText != null)
        {
            coinsText.text = $"Coins: {coins}";
        }
    }
}
