using UnityEngine;

public class CanonController : MonoBehaviour
{
    public GameObject bulletPrefab;  // Bullet prefab reference
    public Transform shootingPoint;  // The point from which the bullet is shot
    public AudioSource audioSource;

    public float fireRate = 2f;  // Time between shots
    private float nextFireTime = 0f;  // Keeps track of when the next shot can be fired


    void Update()
    {
        if (WaveSpawner.isShopActive) return;
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            audioSource.Play();
            ShootBullet();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void ShootBullet()
    {
        // Instantiate the bullet at the shooting point, aligned with the canon's direction
        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);
    }

    public void UpgradeRPM(int rpmLevel)
    {
        // Adjust the fire rate based on the upgrade level
        switch (rpmLevel)
        {
            case 1:
                fireRate = 4f; // Faster firing rate
                Debug.Log("RPM upgraded to level 1: Faster fire rate!");
                break;
            case 2:
                fireRate = 6f; // Even faster firing rate
                Debug.Log("RPM upgraded to level 2: Even faster fire rate!");
                break;
            case 3:
                fireRate = 8f; // Very fast firing rate
                Debug.Log("RPM upgraded to level 3: Very fast fire rate!");
                break;
            case 4:
                fireRate = 10f; // Super fast firing rate
                Debug.Log("RPM upgraded to level 4: Super fast fire rate!");
                break;
            default:
                fireRate = 2f; // Default fire rate
                Debug.Log("RPM is at its base level.");
                break;
        }

        Debug.Log($"RPM upgraded to {fireRate} (Level {rpmLevel})");
    }
}