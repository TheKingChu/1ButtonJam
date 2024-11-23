using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public float timeBetweenWaves = 5f;
    public int initialWaveSize = 10;

    public ShopManager shopManager;

    private int waveNumber = 0;
    private bool isShopActive = false;
    private bool waveInProgress = false;

    // Update is called once per frame
    void Update()
    {
        if(!isShopActive && !waveInProgress)
        {
            StartCoroutine(StartWaveAfterDelay());
        }
    }

    private IEnumerator StartWaveAfterDelay()
    {
        waveInProgress = true;
        yield return new WaitForSeconds(timeBetweenWaves);
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
        OpenShop();
    }

    private void SpawnEnemy()
    {
        GameObject enemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);
    }

    private void OpenShop()
    {
        isShopActive = true;
        shopManager.OpenShop();
    }

    public void CloseShop()
    {
        isShopActive = false;
        shopManager.CloseShop();
    }
}
