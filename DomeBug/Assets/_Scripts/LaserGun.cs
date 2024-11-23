using UnityEngine;

public class LaserGun : MonoBehaviour
{
    public LineRenderer laserBeam;
    public int laserDamage = 5;
    public float laserRange = 15f;
    public LayerMask enemyLayer;

    private bool isLaserActive = false;

    void Start()
    {
        laserBeam.enabled = false;
    }

    void Update()
    {
        if (isLaserActive)
        {
            FireLaser();
        }
    }

    public void ActivateLaser()
    {
        isLaserActive = true;
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

}
