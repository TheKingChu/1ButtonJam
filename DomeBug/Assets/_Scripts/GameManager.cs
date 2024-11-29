using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject domeHolderPrefab;  // Reference to the DomeHolder prefab
    private GameObject domeHolderObject;  // Reference to the DomeHolder object
    public GameObject laserGunPrefab;
    private GameObject laserGunObject;

    public int currentWave = 1; // Start at wave 1
    public int playerCoins = 0;
    public int playerHealth = 100;

    public int[] upgradeLevels = new int[3];

    public TMP_Text coinText;

    //private bool isInitialized = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ensure it persists between scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }

        upgradeLevels[0] = 0; //RPM
        upgradeLevels[1] = 1; //CANONS
        upgradeLevels[2] = 0; //LASER

        // Check if DomeHolder is already in the scene (due to DontDestroyOnLoad)
        domeHolderObject = GameObject.Find("DomeHolder");

        if (domeHolderObject == null)
        {
            // If DomeHolder doesn't exist in the scene, instantiate it from the prefab
            domeHolderObject = Instantiate(domeHolderPrefab);
            domeHolderObject.name = "DomeHolder"; // Ensure it has the correct name
            DontDestroyOnLoad(domeHolderObject); // Keep it across scenes
        }
        else
        {
            // If DomeHolder already exists (because of DontDestroyOnLoad), prevent instantiation
            Debug.Log("DomeHolder already exists, skipping instantiation.");
        }

        laserGunObject = GameObject.Find("LaserHolder");

        if(laserGunObject == null)
        {
            laserGunObject = Instantiate(laserGunPrefab);
            laserGunObject.name = "LaserHolder";
            DontDestroyOnLoad(laserGunObject);
        }
        else
        {
            Debug.Log("LaserHolder already exists, skipping instantiation");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Remove the listener when the object is destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "game") // Ensure we are in the game scene
        {
            InitializeGameObjects(); // Reinitialize game objects for the game scene
        }
    }

    void Start()
    {
        InitializeGameObjects();
    }

    public void InitializeGameObjects()
    {
        // Start a coroutine to wait for CanonController and other objects to be available
        StartCoroutine(WaitForCanonController());
    }

    private IEnumerator WaitForCanonController()
    {
        // Wait until both the ArchController and CanonController are fully initialized
        ArchController archController = domeHolderObject.GetComponentInChildren<ArchController>();
        CanonController canonController = archController.GetComponentInChildren<CanonController>();

        while (archController == null || canonController == null)
        {
            yield return null;  // Wait until the next frame before checking again
            archController = domeHolderObject.GetComponentInChildren<ArchController>();
            canonController = archController.GetComponentInChildren<CanonController>();
        }

        Debug.Log("All game objects initialized");
    }

    public void UpgradeLasers(int newLaserLevel)
    {
        if (laserGunObject == null)
        {
            Debug.LogError("LaserHolder object not found!");
            return;
        }

        // Clamp the laser level to the expected range (0-3 for example)
        newLaserLevel = Mathf.Clamp(newLaserLevel, 0, 4);

        // Find the Laser1 and Laser2 objects within the LaserHolder
        Transform laser1 = laserGunObject.transform.Find("LaserGun1");
        Transform laser2 = laserGunObject.transform.Find("LaserGun2");

        if (laser1 == null || laser2 == null)
        {
            Debug.LogError("LaserGun1 or LaserGun2 not found in LaserHolder!");
            return;
        }

        // Reference the LaseGun components
        LaserGun laserGun1 = laser1.GetComponent<LaserGun>();
        LaserGun laserGun2 = laser2.GetComponent<LaserGun>();

        if (laserGun1 == null || laserGun2 == null)
        {
            Debug.LogError("LaserGun components not found on LaserGun1 or LaserGun2!");
            return;
        }

        // Activate lasers based on the new level
        switch (newLaserLevel)
        {
            case 1: // Activate Laser1
                laser1.gameObject.SetActive(true);
                laser2.gameObject.SetActive(false);
                laserGun1.laserRange = 25f;
                laserGun1.fireDelay = 5f;
                break;
            case 2: // Keep Laser1 active
                laser1.gameObject.SetActive(true);
                laser2.gameObject.SetActive(false);
                laserGun1.laserRange = 100f;
                laserGun1.fireDelay = 1f;
                break;
            case 3: // Activate both Laser1 and Laser2
                laser1.gameObject.SetActive(true);
                laser2.gameObject.SetActive(true);
                laserGun2.laserRange = 25f;
                laserGun2.fireDelay = 5f;
                break;
            case 4:
                laser1.gameObject.SetActive(true);
                laser2.gameObject.SetActive(true);
                laserGun2.laserRange = 100f;
                laserGun2.fireDelay = 1f;
                break;
            default: // No lasers active for level 0 or out of range
                laser1.gameObject.SetActive(false);
                laser2.gameObject.SetActive(false);
                break;
        }

        Debug.Log($"Lasers upgraded to level {newLaserLevel}. Laser1 active: {laser1.gameObject.activeSelf}, Laser2 active: {laser2.gameObject.activeSelf}");
    }



    private void Update()
    {
        UpdateCoinUI();
    }

    public void ResetGame()
    {
        currentWave = 1;
        playerCoins = 0;
        playerHealth = 100;
        upgradeLevels = new int[] { 0, 0, 0 };
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = $"Coins: {playerCoins}";
        }
    }

    public bool IsInitialized()
    {
        return domeHolderPrefab != null && upgradeLevels != null;
    }
}