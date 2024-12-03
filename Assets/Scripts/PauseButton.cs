using UnityEngine;
using UnityEngine.UI; // Required for UI components

public class PauseButton : MonoBehaviour
{
    [Tooltip("The GameObject to activate and deactivate when pausing/unpausing.")]
    public GameObject targetObject;

    [Tooltip("The Button component to toggle pause state.")]
    public Button pauseButton;

    private bool isPaused = false;

    private void Start()
    {
        // Ensure the Button has an OnClick listener assigned
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }
    }

    // Toggles the paused state
    public void TogglePause()
    {
        if (isPaused)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }
    }

    // Pauses the game and activates the target object
    private void PauseGame()
    {
        Time.timeScale = 0; // Freezes game time
        isPaused = true;
        if (targetObject != null)
        {
            targetObject.SetActive(true); // Activate the target object
        }
    }

    // Unpauses the game and deactivates the target object
    private void UnpauseGame()
    {
        Time.timeScale = 1; // Resumes game time
        isPaused = false;
        if (targetObject != null)
        {
            targetObject.SetActive(false); // Deactivate the target object
        }
    }

    private void OnDestroy()
    {
        // Clean up the listener to avoid potential errors
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
        }
    }
}
