using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public delegate void EnemyDestroyedHandler(GameObject enemy);
    public event EnemyDestroyedHandler OnEnemyDestroyed;

    public int baseHealth = 1;
    public int baseDamage = 5;
    public int coinReward = 1;

    private int health;
    private int damage;

    public GameObject deathEffect;

    private bool isPaused = false;

    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;

    private float healthMultiplier = 1f;
    private float damageMultiplier = 1f;

    private void Start()
    {
        SetEnemyDifficulty(healthMultiplier, damageMultiplier);
    }

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
        // Calculate direction to the dome
        Vector3 direction = (dome.position - transform.position).normalized;

        // Rotate the enemy towards the dome
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Adjust rotation to ensure correct orientation (Fix the -90 or -180 issue)
        targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
        
        // Move the enemy towards the dome
        transform.SetPositionAndRotation(Vector3.MoveTowards(transform.position, dome.position, Time.deltaTime * moveSpeed), 
            Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed
        ));
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

    public void SetEnemyDifficulty(float healthMult, float damageMult)
    {
        healthMultiplier = healthMult;
        damageMultiplier = damageMult;

        health = Mathf.RoundToInt(baseHealth * healthMultiplier);
        damage = Mathf.RoundToInt(baseDamage * damageMultiplier);
    }
}
