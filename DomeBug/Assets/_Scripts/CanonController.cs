using UnityEngine;

public class CanonController : MonoBehaviour
{
    public GameObject bulletPrefab;  // Bullet prefab reference
    public Transform shootingPoint;  // The point from which the bullet is shot
    public float fireRate = 1f;  // Time between shots
    private float nextFireTime = 0f;  // Keeps track of when the next shot can be fired

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
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
        rpmLevel++;
        UpdateFireRate();
    }

    void UpdateFireRate()
    {
        fireRate = 1 * 0.5f; //adding 0.5 for each upgrade
    }
}
