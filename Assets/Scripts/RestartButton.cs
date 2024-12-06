using UnityEngine;
using UnityEngine.UI; // Required for UI elements
using UnityEngine.SceneManagement; // Required for scene management

public class RestartButton : MonoBehaviour
{
    // Public button variable to assign in the Unity editor
    public Button restartButton;

    void Start()
    {
        // Ensure the button is not null before adding the listener
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartScene);
        }
        else
        {
            Debug.LogError("Restart Button not assigned!");
        }
    }

    // Method to restart the current scene
    public void RestartScene()
    {
        // Get the current active scene and reload it
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
