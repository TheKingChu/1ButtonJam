using UnityEngine;

public class LaserGun : MonoBehaviour
{
    public LineRenderer laserBeam;
    public int laserDamage = 5;
    public float laserRange = 15f;
    public LayerMask enemyLayer;
    public Transform secondLaserFirePoint; // Position for the second laser (optional)

    private bool isLaserActive = false;
    private int laserGunLevel = 0; // To track the upgrade level

    void Start()
    {
        laserBeam.enabled = false;
        if (secondLaserFirePoint != null)
        {
            // Optionally, deactivate the second laser until the player upgrades it
            secondLaserFirePoint.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isLaserActive)
        {
            FireLaser();

            // If there is a second laser upgrade, handle it
            if (laserGunLevel >= 2 && secondLaserFirePoint != null)
            {
                FireSecondLaser();
            }
        }
    }

    public void ActivateLaser()
    {
        isLaserActive = true;
    }

    // Method to upgrade the laser gun
    public void UpgradeLaser(int upgradeLevel)
    {
        laserGunLevel = upgradeLevel;

        if (laserGunLevel == 1)
        {
            // Activate laser gun on first upgrade
            ActivateLaser();
        }
        else if (laserGunLevel == 2)
        {
            // Second laser added
            if (secondLaserFirePoint != null)
            {
                secondLaserFirePoint.gameObject.SetActive(true);
            }
        }
        else if (laserGunLevel == 3)
        {
            // Increase laser damage (damage boost)
            laserDamage = 15; // Set to higher damage
        }
        else if (laserGunLevel == 4)
        {
            // Increase damage for both lasers
            laserDamage = 20; // Max damage
        }
    }

    void FireLaser()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, laserRange, enemyLayer))
        {
            laserBeam.SetPosition(0, transform.position);
            laserBeam.SetPosition(1, hit.point);

            // Damage the enemy
            EnemyBehavior enemy = hit.collider.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                enemy.TakeDamage(laserDamage);
            }

            laserBeam.enabled = true;
        }
        else
        {
            laserBeam.enabled = false;
        }
    }

    // Fire the second laser if available (after second upgrade)
    void FireSecondLaser()
    {
        RaycastHit hit;
        if (Physics.Raycast(secondLaserFirePoint.position, secondLaserFirePoint.forward, out hit, laserRange, enemyLayer))
        {
            laserBeam.SetPosition(0, secondLaserFirePoint.position);
            laserBeam.SetPosition(1, hit.point);

            // Damage the enemy
            EnemyBehavior enemy = hit.collider.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                enemy.TakeDamage(laserDamage);
            }

            laserBeam.enabled = true;
        }
        else
        {
            laserBeam.enabled = false;
        }
    }
}
