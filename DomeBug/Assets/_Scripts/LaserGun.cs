using UnityEngine;

public class LaserGun : MonoBehaviour
{
    public GameObject laserPrefab; // Assign the laser beam prefab (a cylinder or similar object)
    public Transform firePoint; // The starting point for the laser
    public Transform laserTop; // The rotating top part of the laser gun
    public string enemyTag = "Enemy"; // Tag assigned to enemies
    public float laserRange = 15f; // Maximum range of the laser
    public int laserDamage = 5; // Damage dealt per second
    public float rotationSpeed = 2f; // Speed at which the laser gun rotates towards the target
    public float fireDelay = 1f; // Delay before firing after aiming

    private GameObject currentLaser; // Active laser instance
    private Transform currentTarget; // The closest enemy
    private float fireTimer = 0f; // Timer to handle the fire delay
    private bool isFiring = false; // Whether the laser is currently firing

    void Update()
    {
        // Find the closest enemy
        currentTarget = FindClosestEnemy();

        if (currentTarget != null)
        {
            AimAtTarget(currentTarget);

            // If aiming is complete, prepare to fire
            if (IsAimingAtTarget(currentTarget))
            {
                fireTimer += Time.deltaTime;

                if (fireTimer >= fireDelay && !isFiring)
                {
                    StartFiring();
                }
            }
            else
            {
                // Reset the fire timer if not fully aimed
                fireTimer = 0f;
            }
        }
        else
        {
            // Disable the laser if no target is found
            StopFiring();
        }
    }

    void AimAtTarget(Transform target)
    {
        Vector3 direction = target.position - laserTop.position; // Calculate direction to the target
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        laserTop.rotation = Quaternion.Slerp(
            laserTop.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }

    bool IsAimingAtTarget(Transform target)
    {
        Vector3 directionToTarget = (target.position - laserTop.position).normalized;
        float dotProduct = Vector3.Dot(laserTop.forward, directionToTarget);
        return dotProduct > 0.99f; // Close to perfectly aligned
    }

    void StartFiring()
    {
        isFiring = true;

        if (currentLaser == null)
        {
            // Instantiate the laser beam
            currentLaser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
        }

        // Start updating the laser
        FireLaser(currentTarget.position);
    }

    void FireLaser(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - firePoint.position;
        float distance = Mathf.Min(direction.magnitude, laserRange);

        // Position the laser at the midpoint and adjust its scale
        currentLaser.transform.position = firePoint.position + direction.normalized * (distance / 2);
        currentLaser.transform.localScale = new Vector3(
            currentLaser.transform.localScale.x,
            distance / 2, // Adjust length to match the distance
            currentLaser.transform.localScale.z
        );

        // Rotate the laser to face the target
        currentLaser.transform.rotation = Quaternion.LookRotation(direction);

        // Damage the enemy over time
        DamageEnemy(currentTarget);
    }

    void StopFiring()
    {
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
        // Apply damage to the enemy over time
        EnemyBehavior enemyBehavior = enemy.GetComponent<EnemyBehavior>();
        if (enemyBehavior != null)
        {
            enemyBehavior.TakeDamage((int)(laserDamage * Time.deltaTime)); // Continuous damage
        }
    }

    Transform FindClosestEnemy()
    {
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
