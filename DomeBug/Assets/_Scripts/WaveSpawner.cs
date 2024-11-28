using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveSpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public float timeBetweenWaves = 5f;
    public int initialWaveSize = 10;

    public ShopManager shopManager;
    public WaveManager waveManager;

    private int waveNumber = 0;
    public static bool isShopActive = false;
    private bool waveInProgress = false;
    private bool isWaveSpawnerNotified = false;

    private List<GameObject> activeEnemies = new List<GameObject>();

    public ArchController archController;
    [SerializeField] private CanonController canonController;

    private void Start()
    {
        canonController = FindObjectOfType<CanonController>();
        archController = FindObjectOfType<ArchController>();
    }

    // Update is called once per frame
    void Update()
    {
        canonController = FindObjectOfType<CanonController>();
        archController = FindObjectOfType<ArchController>();

        if (!isShopActive && !waveInProgress && activeEnemies.Count == 0)
        {
            StartCoroutine(StartWaveAfterDelay());
        }
    }

    private IEnumerator StartWaveAfterDelay()
    {
        waveInProgress = true;
        waveManager.StartWave();

        yield return new WaitForSeconds(2f);
        yield return new WaitForSeconds(timeBetweenWaves);

        archController.enabled = true;

        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        waveNumber++;
        int waveSize = initialWaveSize + waveNumber;

        for(int i = 0; i < waveSize; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f);
        }

        //end of wave = open shop
        waveInProgress = false;
        StartCoroutine(CheckForEnemies());
    }

    private void SpawnEnemy()
    {
        GameObject enemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject spawnedEnemy = Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);

        Renderer enemyRenderer = spawnedEnemy.GetComponent<Renderer>();
        if(enemyRenderer != null)
        {
            enemyRenderer.material.color = Random.ColorHSV(0f, 1f, 0.8f, 1, 0.8f, 1f);
        }

        activeEnemies.Add(spawnedEnemy);
        spawnedEnemy.GetComponent<EnemyBehavior>().OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private IEnumerator CheckForEnemies()
    {
        while(activeEnemies.Count > 0)
        {
            yield return null;
        }
        GameManager.Instance.currentWave++;
        SceneManager.LoadScene("shop");
    }

    private void HandleEnemyDestroyed(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }
}
