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

    public static bool isShopActive = false;
    private bool waveInProgress = false;
    private List<GameObject> activeEnemies = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
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

        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        int waveNumber = GameManager.Instance.currentWave;

        // Define the difficulty multipliers for health and damage based on the wave number
        float healthMultiplier = 1 + (waveNumber * 2f);  // Increase health by 10% per wave
        float damageMultiplier = 1 + (waveNumber * 2f);  // Increase damage by 5% per wave

        int waveSize = initialWaveSize + waveNumber;
        for (int i = 0; i < waveSize; i++)
        {
            SpawnEnemy(healthMultiplier, damageMultiplier);
            yield return new WaitForSeconds(0.5f);
        }

        //end of wave = open shop
        waveInProgress = false;
        StartCoroutine(CheckForEnemies());
    }

    private void SpawnEnemy(float healthMultiplier, float damageMultiplier)
    {
        GameObject enemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject spawnedEnemy = Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);

        EnemyBehavior enemyBehavior = spawnedEnemy.GetComponent<EnemyBehavior>();
        enemyBehavior.SetEnemyDifficulty(healthMultiplier, damageMultiplier);

        Renderer[] enemyRenderers = spawnedEnemy.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in enemyRenderers)
        {
            renderer.material.color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
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
