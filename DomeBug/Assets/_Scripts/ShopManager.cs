using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public GameObject shopUI;

    public GameObject archController;
    private ArchController arch;

    [Header("Upgrades")]
    public int rpmLevel = 0, canonsLevel = 0, laserLevel = 0;
    public int maxUpgradeLevel = 5;

    [Header("Costs")]
    private int[] rpmCosts = { 5, 10, 15, 20, 25 };
    private int[] canonCosts = { 10, 20, 30, 40, 50 };
    private int[] laserCosts = { 25, 50, 75, 100, 125 };

    private int selectedItemIndex = 0;
    private bool isHolding = false;
    private float holdTime = 1f;
    private float holdProgress = 0f;
    private bool isShopClosed = false;

    private bool isLocked = false;
    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;
    private bool awaitingSecondTap = false;

    [Header("UI")]
    public Button[] itemButtons;
    public Image[] upgradeBars;
    public TMP_Text[] upgradeLevelTexts;
    public Outline[] buttonOutlines;

    // Reference to the WaveSpawner to signal shop closure
    public WaveSpawner waveSpawner;

    void Start()
    {
        shopUI.SetActive(false);
        arch = archController.GetComponent<ArchController>();
        UpdateUI();
    }

    public void OpenShop()
    {
        shopUI.SetActive(true);
        SelectItem(0);
        ResetHold();
        isShopClosed = false;
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        if (isShopClosed) return; // Prevent redundant calls
        isShopClosed = true;

        waveSpawner.CloseShop();
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
        // Navigate through items with a tap of the Space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float currentTime = Time.time;
            if(awaitingSecondTap && currentTime - lastTapTime < doubleTapThreshold)
            {
                ToggleLock();
                awaitingSecondTap = false;
            }
            else
            {
                awaitingSecondTap = true;
                StartCoroutine(ProcessSingleTapWithDelay());
            }

            lastTapTime = currentTime;
        }

        // Hold Space to purchase the current item
        if (Input.GetKey(KeyCode.Space))
        {
            if(isLocked && !isHolding)
            {
                holdProgress += Time.deltaTime / holdTime;
                holdProgress = Mathf.Clamp01(holdProgress);
                upgradeBars[selectedItemIndex].fillAmount = holdProgress;

                if (holdProgress >= 1f)
                {
                    if (UpgradeSelectedItem())
                    {
                        waveSpawner.CloseShop(); // Close shop after a successful purchase
                    }
                    ResetHold();
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            ResetHold();
        }
    }

    private IEnumerator ProcessSingleTapWithDelay()
    {
        yield return new WaitForSeconds(doubleTapThreshold);

        if (awaitingSecondTap)
        {
            awaitingSecondTap = false;
            if (!isLocked)
            {
                CycleSelection();
            }
        }
    }

    private void CycleSelection()
    {
        if (isLocked) return;

        selectedItemIndex = (selectedItemIndex + 1) % itemButtons.Length;
        SelectItem(selectedItemIndex);
        ResetHold();
        Debug.Log($"Selected Item: {selectedItemIndex}");
    }

    private void ToggleLock()
    {
        isLocked = !isLocked;
        if (isLocked)
        {
            Debug.Log($"locked on item: {selectedItemIndex}");
        }
        else
        {
            Debug.Log("unlocked");
        }
    }

    private void ResetHold()
    {
        isHolding = false;
        holdProgress = 0f;

        foreach (var bar in upgradeBars)
        {
            bar.fillAmount = 0f;
        }
    }
    private void SelectItem(int index)
    {
        for (int i = 0; i < buttonOutlines.Length; i++)
        {
            buttonOutlines[i].enabled = (i == index); // Highlight the selected button
        }
    }


    private bool UpgradeSelectedItem()
    {
        int playerCoins = PlayerStats.coins;
        int cost = GetCurrentUpgradeCost();

        if (playerCoins >= cost)
        {
            PlayerStats.SpendCoins(cost);

            switch (selectedItemIndex)
            {
                case 0: // RPM
                    if (rpmLevel < maxUpgradeLevel)
                    {
                        rpmLevel++;
                        break;
                    }
                    return false;
                case 1: // Canons
                    if (canonsLevel < maxUpgradeLevel)
                    {
                        canonsLevel++;
                        break;
                    }
                    return false;
                case 2: // Laser
                    if (laserLevel < maxUpgradeLevel)
                    {
                        laserLevel++;
                        break;
                    }
                    return false;
                default:
                    return false;
            }

            UpdateUI();
            return true; // Upgrade successful
        }

        return false; // Not enough coins
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

    private void UpdateUI()
    {
        upgradeLevelTexts[0].text = $"RPM: {rpmLevel}/{maxUpgradeLevel}";
        upgradeLevelTexts[1].text = $"Canons: {canonsLevel}/{maxUpgradeLevel}";
        upgradeLevelTexts[2].text = $"Laser: {laserLevel}/{maxUpgradeLevel}";
    }
}
