using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public int health = 1;
    public int damage = 5;
    public int coinReward = 1;

    public GameObject deathEffect;


    // Update is called once per frame
    void Update()
    {
        MoveTowardsDome();
    }

    private void MoveTowardsDome()
    {
        Transform dome = GameObject.FindGameObjectWithTag("Dome").transform;
        transform.position = Vector3.MoveTowards(transform.position, dome.position, Time.deltaTime * 2f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dome"))
        {
            DomeHealth domeHealth = collision.gameObject.GetComponent<DomeHealth>();
            domeHealth.TakeDamage(damage);
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Bullet"))
        {
            Die();
        }
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if(health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        PlayerStats.coins += coinReward;
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
