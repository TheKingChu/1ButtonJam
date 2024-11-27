using UnityEngine;

public class CanonController : MonoBehaviour
{
    public GameObject bulletPrefab;  // Bullet prefab reference
    public Transform shootingPoint;  // The point from which the bullet is shot
    public AudioSource audioSource;
    public float fireRate = 1f;  // Time between shots
    private float fireRateModifier = 1.0f;
    private float nextFireTime = 0f;  // Keeps track of when the next shot can be fired

    private readonly float[] rpmModifiers = new float[] { 4f, 6f, 8f, 10f };

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
        if (rpmLevel >= rpmModifiers.Length)
        {
            // If the level is too high, keep the max modifier
            fireRateModifier = rpmModifiers[rpmModifiers.Length - 1];
        }
        else
        {
            fireRateModifier = rpmModifiers[rpmLevel];
        }

        UpdateFireRate();
    }

    private void UpdateFireRate()
    {
        // Adjust the fire rate based on the modifier (this can be used in firing logic)
        float newFireRate = fireRate * fireRateModifier;
        Debug.Log($"New Fire Rate: {newFireRate}");
    }
}
