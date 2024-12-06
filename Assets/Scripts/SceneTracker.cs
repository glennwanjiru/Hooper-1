using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTracker : MonoBehaviour
{
    // This method will be called when the scene starts
    void Start()
    {
        // Get the current scene index
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Mark this scene as opened in PlayerPrefs
        PlayerPrefs.SetInt("Scene" + sceneIndex + "Opened", 1);
        PlayerPrefs.Save();

        Debug.Log("Scene " + sceneIndex + " has been marked as opened.");
    }

    // Optional: Call this method to manually check if the scene has been opened
    public static bool HasSceneBeenOpened(int sceneIndex)
    {
        return PlayerPrefs.GetInt("Scene" + sceneIndex + "Opened", 0) == 1;
    }
}
