using UnityEngine;

public class LaserGun : MonoBehaviour
{
    public GameObject laserPrefab; // Prefab for the laser beam (cylinder or similar object)
    public Transform firePoint; // Point where the laser originates
    public Transform laserTop; // Part of the gun that rotates towards the target
    public string enemyTag = "Enemy"; // Tag to identify enemies
    public float laserRange = 15f; // Maximum range of the laser
    public int laserDamage = 5; // Damage dealt per second
    public float rotationSpeed = 5f; // Speed at which the gun rotates towards the target
    public float fireDelay = 0.5f; // Delay before the laser starts firing

    private GameObject currentLaser; // Active instance of the laser beam
    private Transform currentTarget; // The closest enemy
    private bool isFiring = false; // Whether the laser is actively firing
    private float fireTimer = 0f; // Timer to handle the delay before firing

    void Update()
    {
        // Find the closest enemy within range
        currentTarget = FindClosestEnemy();

        if (currentTarget != null)
        {
            // Aim at the target
            AimAtTarget(currentTarget);

            // Check if the gun is aligned with the target
            if (IsAimingAtTarget(currentTarget))
            {
                // Fire the laser after the delay
                fireTimer += Time.deltaTime;
                if (fireTimer >= fireDelay)
                {
                    FireLaser(currentTarget.position);
                    DamageEnemy(currentTarget);
                }
            }
            else
            {
                // Reset the firing timer while aiming
                fireTimer = 0f;
            }
        }
        else
        {
            // Stop firing if no target is found
            StopFiring();
        }
    }

    void AimAtTarget(Transform target)
    {
        // Calculate the direction to the target
        Vector3 direction = target.position - laserTop.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate the laser top
        laserTop.rotation = Quaternion.Slerp(
            laserTop.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );

        Debug.DrawRay(laserTop.position, laserTop.forward * 5, Color.blue); // Shows where the laserTop is pointing
        Debug.DrawRay(laserTop.position, (target.position - laserTop.position).normalized * 5, Color.red); // Shows the direction to the target

    }

    bool IsAimingAtTarget(Transform target)
    {
        // Check if the gun is aligned with the target
        Vector3 directionToTarget = (target.position - laserTop.position).normalized;
        float dotProduct = Vector3.Dot(laserTop.forward, directionToTarget);

        // Return true if almost perfectly aligned
        return dotProduct > 0.99f;
    }

    void FireLaser(Vector3 targetPosition)
    {
        if (currentLaser == null)
        {
            // Instantiate the laser beam if it doesn't exist
            currentLaser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
        }

        // Calculate the direction and distance to the target
        Vector3 direction = targetPosition - firePoint.position;
        float distance = direction.magnitude;

        // Position the laser at the midpoint between the fire point and the target
        currentLaser.transform.position = firePoint.position + direction / 2;

        // Scale the laser to match the distance
        currentLaser.transform.localScale = new Vector3(currentLaser.transform.localScale.x, distance / 2, currentLaser.transform.localScale.z);

        // Rotate the laser to face the target, accounting for Unity cylinder default alignment
        Quaternion laserRotation = Quaternion.LookRotation(direction);
        laserRotation *= Quaternion.Euler(90, 0, 0); // Adjust rotation for cylinder's Y-axis alignment
        currentLaser.transform.rotation = laserRotation;

        // Set the laser as active
        isFiring = true;
    }

    void StopFiring()
    {
        // Reset the firing state and destroy the laser beam
        isFiring = false;
        fireTimer = 0f;

        if (currentLaser != null)
        {
            Destroy(currentLaser);
            currentLaser = null;
        }
    }

    void DamageEnemy(Transform enemy)
    {
        // Apply damage to the enemy continuously
        EnemyBehavior enemyBehavior = enemy.GetComponent<EnemyBehavior>();
        if (enemyBehavior != null)
        {
            enemyBehavior.TakeDamage(Mathf.CeilToInt(laserDamage * Time.deltaTime)); // Continuous damage over time
        }
    }

    Transform FindClosestEnemy()
    {
        // Find all enemies in the scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(firePoint.position, enemy.transform.position);
            if (distance < closestDistance && distance <= laserRange)
            {
                closestEnemy = enemy.transform;
                closestDistance = distance;
            }
        }

        return closestEnemy;
    }
}
