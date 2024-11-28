using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public delegate void EnemyDestroyedHandler(GameObject enemy);
    public event EnemyDestroyedHandler OnEnemyDestroyed;

    public int health = 1;
    public int damage = 5;
    public int coinReward = 1;

    public GameObject deathEffect;

    private bool isPaused = false;

    // Update is called once per frame
    void Update()
    {
        // Check if the shop is active, if so, pause the enemy behavior
        if (WaveSpawner.isShopActive)
        {
            if (!isPaused)
            {
                PauseEnemy();
            }
        }
        else
        {
            // If the shop is not active, resume the enemy behavior
            if (isPaused)
            {
                ResumeEnemy();
            }

            // Normal behavior (moving towards the dome)
            MoveTowardsDome();
        }
    }

    private void PauseEnemy()
    {
        isPaused = true;
        // Stop enemy movement by freezing Rigidbody velocity
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;  // Freeze velocity
            rb.isKinematic = true;        // Optionally make Rigidbody kinematic while paused
        }
    }

    private void ResumeEnemy()
    {
        isPaused = false;

        // Ensure Rigidbody is no longer kinematic and resumes normal behavior
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // Allow Rigidbody to be affected by physics again
        }
    }

    private void MoveTowardsDome()
    {
        Transform dome = GameObject.FindGameObjectWithTag("Dome").transform;
        transform.position = Vector3.MoveTowards(transform.position, dome.position, Time.deltaTime * 2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Dome"))
        {
            DomeHealth domeHealth = other.gameObject.GetComponent<DomeHealth>();
            domeHealth.TakeDamage(damage);
            GameManager.Instance.playerHealth = domeHealth.currentHealth;
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            OnEnemyDestroyed?.Invoke(gameObject);
            Destroy(gameObject);
            Destroy(effect, 5f);
        }

        if (other.gameObject.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            Die();
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isPaused) return;  // If paused, don't take damage
        health -= dmg;
        if(health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.playerCoins += coinReward;
        GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        OnEnemyDestroyed?.Invoke(gameObject);
        Destroy(gameObject);
        Destroy(effect, 5f);
    }
}
