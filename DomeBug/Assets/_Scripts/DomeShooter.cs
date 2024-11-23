using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomeShooter : MonoBehaviour
{
    public Transform gun;
    public GameObject bulletPrefab;
    public float rotationSpeed = 50f;
    public float bulletSpeed = 10f;

    private float shootCooldown = 0f;
    private float fireRate = 1f;

    // Update is called once per frame
    void Update()
    {
        RotateGun();
        if (Input.GetKey(KeyCode.Space))
        {
            Shoot();
            shootCooldown = 1f / fireRate;
        }
        shootCooldown -= Time.deltaTime;
    }

    private void RotateGun()
    {
        gun.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, gun.position, gun.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = gun.forward * bulletSpeed;
    }

    public void UpgradeFireRate()
    {
        fireRate += 0.2f; //increase RPM
    }
}
