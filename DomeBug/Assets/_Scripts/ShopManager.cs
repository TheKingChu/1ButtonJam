using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject shopUI;
    public GameObject archController;
    public GameObject laserGunPrefab;
    private ArchController arch;
    private LaserGun laserGun;

    [Header("Upgrades")]
    public int rpmLevel = 0, canonsLevel = 0, laserLevel = 0;
    public int maxUpgradeLevel = 5;

    // Cost for each item upgrade
    private int[] rpmCosts = { 5, 10, 15, 20, 25 };
    private int[] canonCosts = { 10, 20, 30, 40, 50 };
    private int[] laserCosts = { 25, 50, 75, 100, 125 };

    private int selectedItemIndex = 0;
    private bool canCloseShop = false;

    void Start()
    {
        shopUI.SetActive(false);
        arch = archController.GetComponent<ArchController>();
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
            selectedItemIndex = (selectedItemIndex + 1) % 3;  // Rotate between RPM, Canons, and Laser
        }
        if (Input.GetKey(KeyCode.Space))
        {
            if (Upgrade())
            {
                CloseShop();  // Close shop after an upgrade is successful
            }
        }
    }

    bool Upgrade()
    {
        int playerCoins = PlayerStats.coins;
        int cost = GetCurrentUpgradeCost();

        if (playerCoins >= cost)
        {
            if (selectedItemIndex == 0 && rpmLevel < maxUpgradeLevel)
            {
                rpmLevel++;
                PlayerStats.coins -= cost;

                // Apply the new RPM level to the firing rate of all canons
                arch.UpgradeRPM(rpmLevel);
                return true;
            }
            else if (selectedItemIndex == 1 && canonsLevel < maxUpgradeLevel)
            {
                canonsLevel++;
                PlayerStats.coins -= cost;

                // Add a new canon to the arch
                arch.UpgradeCanons(canonsLevel);
                return true;
            }
            else if (selectedItemIndex == 2 && laserLevel < maxUpgradeLevel)
            {
                laserLevel++;
                PlayerStats.coins -= cost;

                if (laserLevel == 1) // Add laser only once (first upgrade)
                {
                    GameObject laser = Instantiate(laserGunPrefab, archController.transform);
                    laserGun = laser.GetComponent<LaserGun>();
                    laserGun.ActivateLaser();  // Activate laser gun
                }
                return true;
            }
        }
        return false;
    }

    private int GetCurrentUpgradeCost()
    {
        switch (selectedItemIndex)
        {
            case 0: // RPM
                return rpmLevel < maxUpgradeLevel ? rpmCosts[rpmLevel] : int.MaxValue;
            case 1: // Canons
                return canonsLevel < maxUpgradeLevel ? canonCosts[canonsLevel] : int.MaxValue;
            case 2: // Laser
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
