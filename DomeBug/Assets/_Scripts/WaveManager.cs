using UnityEngine;
using TMPro;  // Include this if you're using TextMeshPro

public class WaveManager : MonoBehaviour
{
    public int currentWave = 1;  // The current wave number
    public GameObject waveTextUI;  // Reference to the UI Text element for the wave number
    public TextMeshProUGUI waveText;  // If using TextMeshPro, reference the TextMeshProUGUI component
    public float waveTextDuration = 2f;  // Duration to show the wave text

    void Start()
    {
        waveTextUI.SetActive(false);  // Make sure the wave text is hidden initially
    }

    // Call this method to start a new wave
    public void StartWave()
    {
        ShowWaveText();
        // Start spawning enemies or anything related to starting a new wave here
    }

    // Display the wave text for a short time
    private void ShowWaveText()
    {
        waveTextUI.SetActive(true);  // Show the wave text UI
        waveText.text = "Wave " + currentWave;  // Set the wave text

        // Hide the wave text after a delay
        Invoke("HideWaveText", waveTextDuration);
    }

    // Method to hide the wave text
    private void HideWaveText()
    {
        waveTextUI.SetActive(false);
    }

    // Call this method to go to the next wave
    public void NextWave()
    {
        currentWave++;  // Increase the wave number
        StartWave();  // Start the next wave
    }
}
