using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public delegate void EnemyDestroyedHandler(GameObject enemy);
    public event EnemyDestroyedHandler OnEnemyDestroyed;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Dome"))
        {
            DomeHealth domeHealth = other.gameObject.GetComponent<DomeHealth>();
            domeHealth.TakeDamage(damage);
            Instantiate(deathEffect, transform.position, Quaternion.identity);
            OnEnemyDestroyed?.Invoke(gameObject);
            Destroy(gameObject);
        }

        if (other.gameObject.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
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
        PlayerStats.AddCoins(coinReward);
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        OnEnemyDestroyed?.Invoke(gameObject);
        Destroy(gameObject);
    }
}
