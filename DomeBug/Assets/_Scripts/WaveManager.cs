using UnityEngine;
using TMPro;
using System.Collections;  // Include this if you're using TextMeshPro

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
    }

    // Display the wave text for a short time
    private void ShowWaveText()
    {
        waveTextUI.SetActive(true);  // Show the wave text UI
        waveText.text = "Wave " + currentWave;  // Set the wave text

        StartCoroutine(HideWaveText());
    }

    // Method to hide the wave text
    private IEnumerator HideWaveText()
    {
        yield return new WaitForSecondsRealtime(waveTextDuration);
        waveTextUI.SetActive(false);
    }

    // Call this method to go to the next wave
    public void NextWave()
    {
        currentWave++;  // Increase the wave number
        StartWave();  // Start the next wave
    }
}
