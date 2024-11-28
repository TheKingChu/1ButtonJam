using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject domeHolderPrefab;  // Reference to the DomeHolder prefab
    private GameObject domeHolderObject;  // Reference to the DomeHolder object
    public LaserGun laserGun;

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

        upgradeLevels[0] = 0;
        upgradeLevels[1] = 0;
        upgradeLevels[2] = 0;

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

        while (laserGun == null)
        {
            laserGun = FindObjectOfType<LaserGun>();
            if (laserGun != null)
            {
                DontDestroyOnLoad(laserGun.gameObject); // Persist the LaserGun
            }
            yield return null; // Wait until the next frame
        }

        Debug.Log("All game objects initialized");
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