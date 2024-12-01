using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject storeUI; // The main store UI panel
    public TextMeshProUGUI[] upgradeTexts; // Upgrade descriptions and levels
    public Slider[] progressBars; // Progress bars for purchasing
    public Outline[] buttonOutlines; // Outlines for indicating selected item
    public TMP_Text feedbackText; // Text for when you don't have enough money
    public GameObject feedbackObject; // Object for holding text, panel etc...

    public float feedbackDuration = 2f;

    [Header("Upgrades")]
    public int[] upgradeLevels; // Current upgrade levels
    public int[] maxUpgradeLevels; // Maximum upgrade levels for each item
    public int[][] upgradeCosts; // Costs for upgrades (2D array per level)

    [Header("Input Settings")]
    public float holdTime = 1.0f; // Time required to hold for purchase
    public float doubleTapThreshold = 0.3f; // Time window for detecting double tap
    public float purchaseDelay = 0.5f; // Delay before allowing hold to purchase

    private int selectedItemIndex = 0; // Index of the currently selected item
    private bool isLocked = false; // Whether an item is locked for purchase
    private bool isHolding = false;
    private float holdProgress = 0.0f; // Current progress of the hold-to-buy action

    private float lastTapTime = 0.0f;
    private bool awaitingSecondTap = false;
    private bool canBuy = false; 

    private ArchController archController;

    void Start()
    {
        Time.timeScale = 1;
        Debug.Log("ShopManager script is active!");
        feedbackObject.SetActive(false);

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance is missing!");
            return;
        }

        // Initialize upgrade costs
        upgradeCosts = new int[][]
        {
        new int[] { 5, 10, 20, 40 }, // Costs for upgrade 0
        new int[] { 0, 10, 20, 40 }, // Costs for upgrade 1
        new int[] { 15, 30, 60, 120 }  // Costs for upgrade 2
        };

        upgradeLevels = GameManager.Instance.upgradeLevels;
        maxUpgradeLevels = new int[] { 4, 4, 4 }; //setting the max levels

        if (upgradeTexts.Length == 0 || progressBars.Length == 0 || buttonOutlines.Length == 0)
        {
            Debug.LogError("UI elements are not assigned in the Inspector!");
            return;
        }

        GameManager.Instance.IsInitialized();

        archController = FindObjectOfType<ArchController>();
        if (archController != null)
        {
            archController.OnCanonSpawned += HandleCanonSpawned;
        }

        archController.GetComponentInChildren<CanonController>().enabled = false;

        UpdateShopUI();
    }

    void Update()
    {
        UpdateShopUI();
        HandleInput();
    }

    private void CycleSelection()
    {
        if (isLocked) return;

        selectedItemIndex = (selectedItemIndex + 1) % upgradeTexts.Length;
        Debug.Log($"Selected Item: {selectedItemIndex}");
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

    private void HandleCanonSpawned(CanonController canonController)
    {
        Debug.LogWarning("CanonController detected via event!");
        int rpmLevel = GameManager.Instance.upgradeLevels[0];
        Debug.LogWarning($"RPM Level from GameManager: {rpmLevel}");
        canonController.UpgradeRPM(rpmLevel);
    }

    private void OnDestroy()
    {
        if (archController != null)
        {
            archController.OnCanonSpawned -= HandleCanonSpawned; // Unsubscribe to avoid memory leaks
        }
    }

    private void HandleLaserUpgrade()
    {
        int laserUpgradeLevel = upgradeLevels[2]; // Assuming index 2 is for lasers
        GameManager.Instance.UpgradeLasers(laserUpgradeLevel);
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

    private void ToggleLock()
    {
        isLocked = !isLocked;
        Debug.Log(isLocked ? $"Locked on item: {selectedItemIndex}" : "Unlocked");
        buttonOutlines[selectedItemIndex].effectColor = isLocked ? Color.red : Color.cyan;
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
            Debug.Log($"Upgrade {upgradeIndex} purchased. Level: {upgradeLevels[upgradeIndex]}");

            // Handle RPM upgrade
            if (upgradeIndex == 0)
            {
                // Call HandleCanonSpawned for RPM upgrade
                if (archController != null)
                {
                    CanonController canonController = archController.GetComponentInChildren<CanonController>();
                    if (canonController != null)
                    {
                        HandleCanonSpawned(canonController);  // Passing the canonController to the method
                    }
                    else
                    {
                        Debug.LogError("CanonController not found in ArchController!");
                    }
                }
            }
            // Handle adding canons upgrade
            else if (upgradeIndex == 1)
            {
                // Call UpgradeCanons to add more canons based on the new level
                int newCanonLevel = GameManager.Instance.upgradeLevels[1];  // Get the new upgrade level for canons
                Debug.LogWarning($"Upgrading canons to level: {newCanonLevel}");
                if (newCanonLevel > archController.canonCount)  // Assuming canonCount is public or accessible
                {
                    if (archController != null)
                    {
                        archController.UpgradeCanons(newCanonLevel);  // Upgrade only if needed
                    }
                }
                else
                {
                    Debug.LogWarning("No upgrade needed. Current canon level is sufficient.");
                }
            }

            else if(upgradeIndex == 2)
            {
                HandleLaserUpgrade();
            }
        }
        else
        {
            Debug.Log("Not enough coins to purchase the upgrade!");
            StartCoroutine(ShowFeedback("Not enough coins to purchase the upgrade!"));
        }

        UpdateShopUI(); // Refresh UI with updated stats
    }

    private IEnumerator ShowFeedback(string message)
    {
        feedbackText.text = message;
        feedbackObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);  // Ensure the text is visible
        yield return new WaitForSeconds(feedbackDuration);  // Wait for the feedback duration
        feedbackObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);  // Hide the message
    }

    private IEnumerator ShowFeedbackAndReturnToGame(string message)
    {
        feedbackText.text = message;  // Show the message
        feedbackText.gameObject.SetActive(true);  // Ensure the text is visible
        yield return new WaitForSeconds(feedbackDuration);  // Wait for the feedback duration
        feedbackText.gameObject.SetActive(false);  // Hide the message
        ReturnToGame();  // Transition back to the game
    }

    public void ReturnToGame()
    {
        archController.GetComponentInChildren<CanonController>().enabled = true;
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
            new int[] { 0, 10, 20, 40 }, // Upgrade 1
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
            StartCoroutine(ShowFeedbackAndReturnToGame("You don't have enough money!"));
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

        if(!canBuy && !isLocked)
        {
            StartCoroutine(EnablePurchaseAfterDelay());
        }
    }

    private IEnumerator EnablePurchaseAfterDelay()
    {
        // Wait for the specified delay before allowing purchase
        yield return new WaitForSeconds(purchaseDelay);
        canBuy = true;
    }
}