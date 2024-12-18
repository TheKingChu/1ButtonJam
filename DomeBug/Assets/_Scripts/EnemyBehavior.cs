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

    [SerializeField] private int health;
    [SerializeField] private int damage;

    public GameObject deathEffect;
    public AudioSource audioSource;

    private bool isPaused = false;

    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;

    private float healthMultiplier = 1f;
    private float damageMultiplier = 1f;

    private Transform domeTransform;

    private Renderer[] renderers;
    private Color originalColor;
    private bool isFlashing = false;
    public float flashDuration = 0.1f;
    public Color flashColor = Color.white;


    private void Start()
    {
        // Cache the dome transform once at the start to improve performance
        GameObject dome = GameObject.FindGameObjectWithTag("Dome");
        if (dome != null)
        {
            domeTransform = dome.transform;
        }

        renderers = GetComponentsInChildren<Renderer>();
        if(renderers.Length > 0 )
        {
            originalColor = renderers[0].material.color;
        }

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
            if (domeTransform != null)
            {
                MoveTowardsDome();
            }
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
        // Calculate direction to the dome
        Vector3 direction = (domeTransform.position - transform.position).normalized;

        // Rotate the enemy towards the dome
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Adjust rotation to ensure correct orientation (Fix the -90 or -180 issue)
        targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);

        // Move the enemy towards the dome
        transform.SetPositionAndRotation(Vector3.MoveTowards(transform.position, domeTransform.position, Time.deltaTime * moveSpeed),
            Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed)
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Dome"))
        {
            DomeHealth domeHealth = other.gameObject.GetComponent<DomeHealth>();
            if (domeHealth != null)
            {
                domeHealth.TakeDamage(damage);
                GameManager.Instance.playerHealth = domeHealth.currentHealth;
            }

            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            OnEnemyDestroyed?.Invoke(gameObject);
            Destroy(gameObject);
            Destroy(effect, 5f);
        }

        if (other.gameObject.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);  // Destroy bullet
            TakeDamage(3);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isPaused) return;  // If paused, don't take damage
        health -= dmg;
        if (!isFlashing)
        {
            StartCoroutine(FlashEffect());
        }
        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashEffect()
    {
        isFlashing = true;

        // Change the material color to the flash color
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = flashColor;
        }

        yield return new WaitForSeconds(flashDuration);

        // Revert the material color to the original color
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = originalColor;
        }

        isFlashing = false;
    }

    private void Die()
    {
        GameManager.Instance.playerCoins += coinReward;
        GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);

        audioSource.Play();
        Debug.Log("Death sound played.");

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

        Debug.Log("Enemy health set to: " + health + " (Multiplier: " + healthMultiplier + ")");
        Debug.Log("Enemy damage set to: " + damage + " (Multiplier: " + damageMultiplier + ")");
    }
}
