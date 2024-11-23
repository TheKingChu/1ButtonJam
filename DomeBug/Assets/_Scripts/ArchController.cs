using System.Collections.Generic;
using UnityEngine;

public class ArchController : MonoBehaviour
{
    public GameObject canonPrefab;
    public Transform archCenter;
    public float rotationSpeed = 30f;

    private int canonCount = 1;
    private List<GameObject> canons = new List<GameObject>();

    private float archAngle = 180f; // Total angle of the arch (180 degrees)

    void Start()
    {
        Debug.Log("Initial canonCount: " + canonCount);
        AddCanon();  // Initialize the arch with one canon
    }

    void Update()
    {
        // Rotate the arch around the center
        transform.Rotate(Vector3.down, rotationSpeed * Time.deltaTime);
    }

    public void UpgradeCanons()
    {
        canonCount++;  // Increase the canon count
        AddCanon();    // Add a new canon
    }

    void AddCanon()
    {
        // Debugging canonCount before adding a new canon
        Debug.Log("Adding canon with canonCount: " + canonCount);

        // Ensure canonCount is greater than 0 before proceeding
        if (canonCount <= 0)
        {
            Debug.LogError("Canon count must be greater than 0.");
            return;
        }

        // Calculate the angle between each canon based on the total arch angle
        float angleBetweenCanons = archAngle / (float)(canonCount);  // Spread the canons across the 180-degree arc

        // Calculate the angle for the new canon, distributing them over the 180-degree range
        float angle = -archAngle / 2 + (canons.Count) * angleBetweenCanons;

        // Log angle for debugging
        Debug.Log("Calculated angle: " + angle);

        // Ensure the angle is valid and within the expected range
        angle = Mathf.Clamp(angle, -archAngle / 2, archAngle / 2);

        // Generate a quaternion for the rotation around the Y-axis
        Quaternion rotation = Quaternion.Euler(0, angle, 0);

        // Calculate the position for the new canon at a fixed distance (5 units) from the center of the arch
        Vector3 position = rotation * Vector3.forward * 5f;

        // Log position for debugging
        Debug.Log("Calculated position: " + position);

        // Ensure the position is valid before instantiating
        if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
        {
            Debug.LogError("Calculated position is invalid (NaN). Aborting canon instantiation.");
            return;
        }

        // Instantiate the new canon at the calculated position and with the calculated rotation
        GameObject newCanon = Instantiate(canonPrefab, archCenter.position + position, rotation);

        // Attach the new canon to the arch as a child
        newCanon.transform.parent = transform;

        // Add the new canon to the list
        canons.Add(newCanon);
    }



    public void UpgradeRPM(int rpmLevel)
    {
        // Adjust the firing rate of each canon based on the RPM level
        foreach (GameObject canon in canons)
        {
            var controller = canon.GetComponent<CanonController>();
            controller.UpgradeRPM(rpmLevel);  // Assuming CanonController has an UpgradeRPM method
        }
    }

    public void UpgradeCanons(int newCanonLevel)
    {
        // Add canons up to the specified level
        while (canons.Count < newCanonLevel)
        {
            AddCanon();  // Add a new canon
        }
    }
}
