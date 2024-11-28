using System;
using System.Collections.Generic;
using UnityEngine;

public class ArchController : MonoBehaviour
{
    public GameObject canonPrefab;
    public Transform archCenter;
    public float rotationSpeed = 30f;

    public int canonCount = 1;
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
        if (canonCount <= 0)
        {
            Debug.LogWarning("No canons to add. Skipping AddCanon.");
            return;  // Avoid any unnecessary operations if canonCount is non-positive.
        }

        float angleStep = (canonCount > 1) ? 180f / (canonCount - 1) : 180f; // Calculate the spacing of cannons
        Debug.Log($"Adding a canon. Angle step: {angleStep}, Total canons: {canonCount}");

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
        int maxCanonLevel = 4;  // Maximum allowed level for canons
        newCanonLevel = Mathf.Min(newCanonLevel, maxCanonLevel);

        Debug.Log($"UpgradeCanons called. Current canonCount: {canonCount}, Target newCanonLevel: {newCanonLevel}");

        // Step 1: Remove all existing canons
        ClearCanons();

        // Step 2: Recreate canons based on the new upgrade level
        canonCount = newCanonLevel;  // Update canon count to the new level
        Debug.Log($"Recreating canons to match the new level: {canonCount}");

        AddCanonsForCurrentLevel();
    }

    private void ClearCanons()
    {
        // Destroy all existing canons in the scene
        foreach (GameObject canon in canons)
        {
            Destroy(canon);
        }

        // Clear the list to avoid leftover references
        canons.Clear();

        Debug.Log("All existing canons removed.");
    }

    private void AddCanonsForCurrentLevel()
    {
        // Step 3: Add the required number of canons based on the current level
        Debug.Log($"Adding {canonCount} canons to match level {canonCount}.");

        float angleStep = (canonCount > 1) ? 180f / (canonCount - 1) : 180f; // Calculate the spacing of cannons
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

            // Apply the current RPM upgrade level to the new canon
            if (GameManager.Instance != null)
            {
                int rpmLevel = GameManager.Instance.upgradeLevels[0];
                canonController.UpgradeRPM(rpmLevel);
            }
        }
    }
}
