using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;

    void Start()
    {
        // Optionally, add a force or velocity to the bullet
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.up * speed;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Handle collision (e.g., destroy the bullet or apply damage)
        Destroy(gameObject);
    }
}