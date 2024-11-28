using System;
using System.Collections.Generic;
using UnityEngine;

public class ArchController : MonoBehaviour
{
    public GameObject canonPrefab;
    public Transform archCenter;
    public float rotationSpeed = 30f;

    private int canonCount = 1;
    private List<GameObject> canons = new List<GameObject>();
    private float currentAngle = 0f;
    private bool movingRight = true; // To determine the direction of movement

    // Event to notify when a canon is spawned
    public event Action<CanonController> OnCanonSpawned;

    void Start()
    {
        Debug.Log("Initial canonCount: " + canonCount);
        AddCanon();  // Initialize the arch with one canon
    }

    void Update()
    {
        OscillateArch();
    }

    private void AddCanon()
    {
        float angleStep = 180f / canonCount; // Calculate the spacing of cannons
        for (int i = 0; i < canonCount; i++)
        {
            float angle = -90f + i * angleStep; // Start at -90 degrees and distribute over 180 degrees
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 position = archCenter.position + rotation * Vector3.up * 5f; // Adjust radius (distance)

            GameObject newCanon = Instantiate(canonPrefab, position, rotation);
            newCanon.transform.parent = transform;
            canons.Add(newCanon);

            // Notify listeners (e.g., ShopManager) about the new CanonController
            CanonController canonController = newCanon.GetComponent<CanonController>();
            if (canonController != null)
            {
                Debug.Log("Invoking OnCanonSpawned for new CanonController.");
                OnCanonSpawned?.Invoke(canonController); // Trigger the event
            }
            else
            {
                Debug.LogError("CanonController component not found on spawned canon.");
            }

            // Immediately apply the current RPM upgrade level to the new canon
            if (GameManager.Instance != null)
            {
                int rpmLevel = GameManager.Instance.upgradeLevels[0];
                canonController.UpgradeRPM(rpmLevel);
            }
        }
    }

    private void OscillateArch()
    {
        // Determine the direction and update the angle
        if (movingRight)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            if (currentAngle >= 180f) // Limit to +90 degrees
            {
                currentAngle = 180f;
                movingRight = false; // Reverse direction
            }
        }
        else
        {
            currentAngle -= rotationSpeed * Time.deltaTime;
            if (currentAngle <= 0f) // Limit to -90 degrees
            {
                currentAngle = 0f;
                movingRight = true; // Reverse direction
            }
        }

        // Apply the rotation to the arch
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }

    public void UpgradeCanons(int newCanonLevel)
    {
        int maxCanonLevel = 4;
        newCanonLevel = Mathf.Min(newCanonLevel, maxCanonLevel);

        while (canonCount < newCanonLevel)
        {
            canonCount++;
            AddCanon();
        }
    }
}
