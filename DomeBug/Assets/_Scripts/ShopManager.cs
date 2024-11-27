using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject storeUI; // The main store UI panel
    public TextMeshProUGUI[] upgradeTexts; // Upgrade descriptions and levels
    public Slider[] progressBars; // Progress bars for purchasing
    public Outline[] buttonOutlines; // Outlines for indicating selected item

    [Header("Upgrades")]
    public int[] upgradeLevels; // Current upgrade levels
    public int[] maxUpgradeLevels; // Maximum upgrade levels for each item
    public int[][] upgradeCosts; // Costs for upgrades (2D array per level)

    [Header("Input Settings")]
    public float holdTime = 1.0f; // Time required to hold for purchase
    public float doubleTapThreshold = 0.3f; // Time window for detecting double tap

    private int selectedItemIndex = 0; // Index of the currently selected item
    private bool isLocked = false; // Whether an item is locked for purchase
    private bool isHolding = false; // If the player is holding the purchase button
    private float holdProgress = 0.0f; // Current progress of the hold-to-buy action

    private float lastTapTime = 0.0f;
    private bool awaitingSecondTap = false;
    private readonly bool isStoreOpen = true;

    void Start()
    {
        Time.timeScale = 1;
        Debug.Log("ShopManager script is active!");

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance is missing!");
            return;
        }

        // Initialize upgrade costs
        upgradeCosts = new int[][]
        {
        new int[] { 5, 10, 20, 40 }, // Costs for upgrade 0
        new int[] { 10, 20, 40, 80 }, // Costs for upgrade 1
        new int[] { 15, 30, 60, 120 }  // Costs for upgrade 2
        };

        upgradeLevels = GameManager.Instance.upgradeLevels;
        maxUpgradeLevels = new int[] { 4, 4, 4 }; //setting the max levels

        if (upgradeTexts.Length == 0 || progressBars.Length == 0 || buttonOutlines.Length == 0)
        {
            Debug.LogError("UI elements are not assigned in the Inspector!");
            return;
        }

        UpdateShopUI();
    }

    void Update()
    {
        UpdateShopUI();

        if (isStoreOpen)
        {
            HandleInput();
        }
        else
        {
            Debug.Log("Store is not open."); // If this appears, `isStoreOpen` isn't true.
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed");

            float currentTime = Time.unscaledTime;
            if (awaitingSecondTap && currentTime - lastTapTime < doubleTapThreshold)
            {
                awaitingSecondTap = false;
                Debug.Log("Double-tap detected");
                ToggleLock();
            }
            else
            {
                awaitingSecondTap = true;
                Debug.Log("Awaiting second tap...");
                Invoke(nameof(ProcessSingleTap), doubleTapThreshold);
            }
            lastTapTime = currentTime;
        }

        if (Input.GetKey(KeyCode.Space) && isLocked)
        {
            Debug.Log("Space key held for purchase");
            ProcessHold();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("Space key released");
            ResetHold();
        }
    }

    private void CycleSelection()
    {
        if (isLocked) return;

        selectedItemIndex = (selectedItemIndex + 1) % upgradeTexts.Length;
        Debug.Log($"Selected Item: {selectedItemIndex}");
    }

    private void ToggleLock()
    {
        isLocked = !isLocked;
        Debug.Log(isLocked ? $"Locked on item: {selectedItemIndex}" : "Unlocked");
    }

    private void ProcessHold()
    {
        if (upgradeLevels[selectedItemIndex] >= maxUpgradeLevels[selectedItemIndex]) return; // Maxed out

        isHolding = true;
        holdProgress += Time.unscaledDeltaTime / holdTime;
        progressBars[selectedItemIndex].value = Mathf.Clamp01(holdProgress);

        if (holdProgress >= 1.0f)
        {
            PurchaseUpgrade(selectedItemIndex);
            ResetHold();
        }
    }

    private void ResetHold()
    {
        isHolding = false;
        holdProgress = 0.0f;

        // Reset all progress bars
        foreach (var bar in progressBars)
        {
            bar.value = 0.0f;
        }
    }

    private void ProcessSingleTap()
    {
        if (awaitingSecondTap && !isLocked)
        {
            awaitingSecondTap = false;
            CycleSelection();
        }
        else if (!isLocked)
        {
            // Avoid toggling lock during single tap
            awaitingSecondTap = true;
            Invoke(nameof(ResetAwaitingTap), doubleTapThreshold); // Reset awaiting state
        }
    }

    private void ResetAwaitingTap()
    {
        awaitingSecondTap = false; // Clear flag
    }


    public void PurchaseUpgrade(int upgradeIndex)
    {
        int cost = GetUpgradeCost(upgradeIndex);

        if (GameManager.Instance.playerCoins >= cost)
        {
            GameManager.Instance.playerCoins -= cost;
            GameManager.Instance.upgradeLevels[upgradeIndex]++;
            ApplyUpgradeEffect(upgradeIndex);
            Debug.Log($"Upgrade {upgradeIndex} purchased. Level: {upgradeLevels[upgradeIndex]}");
        }
        else
        {
            Debug.Log("Not enough coins to purchase the upgrade!");
        }

        UpdateShopUI(); // Refresh UI with updated stats
    }

    private void ApplyUpgradeEffect(int upgradeIndex)
    {
        // Ensure that the GameManager and objects are properly initialized
        if (!GameManager.Instance.IsInitialized())
        {
            Debug.LogWarning("Game objects not initialized yet.");
            return; // Don't apply upgrade until initialization is complete
        }

        // Apply the corresponding upgrade effect
        switch (upgradeIndex)
        {
            case 0:  // RPM Upgrade
                if (GameManager.Instance.domeHolderPrefab != null)
                {
                    GameManager.Instance.domeHolderPrefab.GetComponentInChildren<CanonController>().UpgradeRPM(GameManager.Instance.upgradeLevels[upgradeIndex]);
                }
                break;
            case 1:  // Add a canon
                if (GameManager.Instance.domeHolderPrefab != null)
                {
                    GameManager.Instance.domeHolderPrefab.GetComponentInChildren<ArchController>().UpgradeCanons(GameManager.Instance.upgradeLevels[upgradeIndex]);
                }
                break;
            case 2:  // Add static laser shooter
                if (GameManager.Instance.laserGun != null)
                {
                    UpgradeLaserGun(GameManager.Instance.upgradeLevels[upgradeIndex]);
                }
                break;
            default:
                break;
        }
    }

    private void UpgradeLaserGun(int laserUpgradeLevel)
    {
        switch (laserUpgradeLevel)
        {
            case 1:
                GameManager.Instance.laserGun.ActivateLaser();
                break;
            case 2:
                //GameManager.Instance.laserGun.UpgradeLaserDamage(10); // Increase laser damage or apply boost
                break;
            case 3:
                // Add another laser
                // Logic to add a second laser
                break;
            case 4:
                // Damage boost for the second laser
                break;
            default:
                break;
        }
    }

    public void ReturnToGame()
    {
        SceneManager.LoadScene("game");
    }

    private int GetUpgradeCost(int upgradeIndex)
    {
        int level = GameManager.Instance.upgradeLevels[upgradeIndex];
        return GetCostForLevel(upgradeIndex, level);
    }

    private int GetCostForLevel(int upgradeIndex, int level)
    {
        // Example costs
        int[][] costs = new int[][]
        {
            new int[] { 5, 10, 20, 40 }, // Upgrade 0
            new int[] { 10, 20, 40, 80 }, // Upgrade 1
            new int[] { 15, 30, 60, 120 }  // Upgrade 2
        };

        return level < costs[upgradeIndex].Length ? costs[upgradeIndex][level] : int.MaxValue;
    }

    private void UpdateShopUI()
    {
        bool canAffordUpgrade = false;
        for (int i = 0; i < upgradeTexts.Length; i++)
        {
            int cost = GetUpgradeCost(i);
            if (GameManager.Instance.playerCoins >= cost && upgradeLevels[i] < maxUpgradeLevels[i])
            {
                canAffordUpgrade = true;
                break;
            }
        }

        // If the player can't afford any upgrade, return to the game
        if (!canAffordUpgrade)
        {
            ReturnToGame();
            return;  // Exit the method early, no need to update the UI if we're going back to the game
        }

        // Update each upgrade UI
        for (int i = 0; i < upgradeTexts.Length; i++)
        {
            int level = upgradeLevels[i];
            string status = level >= maxUpgradeLevels[i] ? "MAX" : $"Cost: {GetUpgradeCost(i)}";
            upgradeTexts[i].text = $"Upgrade {i + 1}\nLevel: {level}/{maxUpgradeLevels[i]}\n{status}";

            // Update progress bar visuals
            progressBars[i].value = 0.0f; // Reset progress bars
        }

        // Update button outlines for selection
        for (int i = 0; i < buttonOutlines.Length; i++)
        {
            buttonOutlines[i].enabled = (i == selectedItemIndex); // Highlight selected
        }
    }
}