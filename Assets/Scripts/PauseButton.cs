using UnityEngine;
using UnityEngine.UI; // Required for UI components

public class PauseButton : MonoBehaviour
{
    [Tooltip("The GameObject to activate and deactivate when pausing/unpausing.")]
    public GameObject targetObject;

    [Tooltip("The Button component to toggle pause state.")]
    public Button pauseButton;

    [Tooltip("The GameObjects that should be disabled when the game is paused.")]
    public GameObject[] disableOnPauseObjects;

    private bool isPaused = false;

    private void Start()
    {
        // Ensure the game is unpaused at the start of each scene
        Time.timeScale = 1;
        isPaused = false;

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
        // Stop all game time
        Time.timeScale = 0; // Freezes game time
        isPaused = true;

        // Disable game-related objects that should be paused
        foreach (var obj in disableOnPauseObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Activate the pause UI elements
        if (targetObject != null)
        {
            targetObject.SetActive(true); // Activate the target object (Pause Menu)
        }

        // Pause button remains interactable
        // No code to disable the button is added
    }

    // Unpauses the game and deactivates the target object
    private void UnpauseGame()
    {
        // Resume game time
        Time.timeScale = 1; // Resumes game time
        isPaused = false;

        // Enable game-related objects that should be active when unpaused
        foreach (var obj in disableOnPauseObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        // Deactivate the pause UI elements
        if (targetObject != null)
        {
            targetObject.SetActive(false); // Deactivate the pause menu
        }

        // Pause button remains interactable
        // No need to re-enable the button since it's never disabled
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
