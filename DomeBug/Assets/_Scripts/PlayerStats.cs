using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStats: MonoBehaviour
{
    public static int coins = 0;
    public TMP_Text coinsText;

    private void Start()
    {
        UpdateCoinUI();
    }

    public static void AddCoins(int amount)
    {
        coins += amount;
        FindObjectOfType<PlayerStats>().UpdateCoinUI();
    }

    public static void SpendCoins(int amount)
    {
        coins -= amount;
        FindObjectOfType<PlayerStats>().UpdateCoinUI();
    }

    private void UpdateCoinUI()
    {
        if (coinsText != null)
        {
            coinsText.text = $"Coins: {coins}";
        }
    }
}
