using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLoader : MonoBehaviour
{
    private void Start()
    {
        RestoreGameState();
    }

    private void RestoreGameState()
    {
        int currentWave = GameManager.Instance.currentWave;

        // Restore any necessary state in your game (e.g., wave number, upgrades)
        Debug.Log($"Starting Wave {currentWave}");
        // Start your wave logic here
    }
}