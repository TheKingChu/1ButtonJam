using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject shopUI;

    [Header("Upgrades")]
    public int rpmLevel = 0, canonsLevel = 0, laserLevel = 0;
    public int maxUpgradeLevel = 5;

    //Cost for each item
    private int[] rpmCosts = { 5, 10, 15, 20, 25 };
    private int[] canonCosts = { 10, 20, 30, 40, 50 };
    private int[] laserCosts = { 25, 50, 75, 100, 125 };

    private int selectedItemIndex = 0;
    private bool canCloseShop = false;

    // Start is called before the first frame update
    void Start()
    {
        shopUI.SetActive(false);
    }

    public void OpenShop()
    {
        shopUI.SetActive(true);
        canCloseShop = false;
        StartCoroutine(AllowShopClosing());
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (shopUI.activeSelf)
        {
            HandleShopInput();
        }
    }

    private void HandleShopInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            selectedItemIndex = (selectedItemIndex + 1) % 3;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            if (Upgrade())
            {
                CloseShop();
            }
        }
    }

    bool Upgrade()
    {
        int playerCoins = PlayerStats.coins;
        int cost = GetCurrentUpgradeCost();

        if(playerCoins >= cost)
        {
            if(selectedItemIndex == 0 && rpmLevel < maxUpgradeLevel)
            {
                rpmLevel++;
                PlayerStats.coins -= cost;
                return true;
            }
            else if(selectedItemIndex == 1 && canonsLevel < maxUpgradeLevel)
            {
                canonsLevel++;
                PlayerStats.coins -= cost;
                return true;
            }
            else if(selectedItemIndex == 2 && laserLevel < maxUpgradeLevel)
            {
                laserLevel++;
                PlayerStats.coins -= cost;
                return true;
            }
        }
        return false;
    }

    private int GetCurrentUpgradeCost()
    {
        switch (selectedItemIndex)
        {
            case 0:
                return rpmLevel < maxUpgradeLevel ? rpmCosts[rpmLevel] : int.MaxValue;
            case 1:
                return canonsLevel < maxUpgradeLevel ? canonCosts[canonsLevel] : int.MaxValue;
            case 2:
                return laserLevel < maxUpgradeLevel ? laserCosts[laserLevel] : int.MaxValue;
            default:
                return int.MaxValue;
        }
    }

    private IEnumerator AllowShopClosing()
    {
        yield return new WaitForSeconds(1f);
        canCloseShop = true;
    }
}
