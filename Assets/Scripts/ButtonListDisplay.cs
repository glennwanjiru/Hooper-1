using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // For scene management

public class ButtonListDisplay : MonoBehaviour
{
    // The Scroll View container where the buttons will be displayed
    public GameObject scrollViewContent;

    // The button prefab to instantiate
    public GameObject buttonPrefab;

    // Number of buttons to display (can be set in the inspector)
    public int numberOfButtons = 5;

    // Starting scene index (public variable to set from inspector)
    public int startingSceneIndex = 3;

    // The message to display if the level hasn't been completed
    public Text messageText;

    void Start()
    {
        // Check if the Scroll View content and Button prefab are assigned
        if (scrollViewContent == null || buttonPrefab == null)
        {
            Debug.LogError("Scroll View Content or Button Prefab not assigned!");
            return;
        }

        // Clear any existing buttons in the content
        foreach (Transform child in scrollViewContent.transform)
        {
            Destroy(child.gameObject);
        }

        // Create buttons dynamically based on the numberOfButtons
        for (int i = 0; i < numberOfButtons; i++)
        {
            // Instantiate a new button from the prefab
            GameObject buttonInstance = Instantiate(buttonPrefab, scrollViewContent.transform);

            // Set the button label to start from 1 and increase
            Text buttonText = buttonInstance.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = (i + 1).ToString(); // Set the label starting from 1
            }

            // Calculate the color intensity using multiple colors (green -> yellow -> red)
            float t = (float)i / (numberOfButtons - 1); // Normalize the index to [0, 1]
            Color buttonColor = GetColorFromGradient(t);

            // Set the button color (assuming the button has an Image component)
            Image buttonImage = buttonInstance.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = buttonColor; // Apply the color to the button's background

                // Check if the scene has been opened
                int sceneIndex = startingSceneIndex + i; // Calculate scene index based on button position
                bool isSceneUnlocked = sceneIndex == startingSceneIndex || SceneTracker.HasSceneBeenOpened(sceneIndex);

                // Set button transparency (0.5 for locked/unopened scenes)
                if (!isSceneUnlocked)
                {
                    Color colorWithTransparency = buttonImage.color;
                    colorWithTransparency.a = 0.2f; // Set alpha to 0.5 for locked scenes
                    buttonImage.color = colorWithTransparency;
                }
            }

            // Add a listener to the button to load the corresponding scene, or display the message
            Button button = buttonInstance.GetComponent<Button>();
            if (button != null)
            {
                int sceneIndex = startingSceneIndex + i; // Calculate scene index based on button position
                button.onClick.AddListener(() => OnButtonClicked(sceneIndex, buttonText));
            }
        }
    }

    // Example method for button click action
    void OnButtonClicked(int sceneIndex, Text buttonText)
    {
        // Check if the scene has been opened
        if (sceneIndex == startingSceneIndex || SceneTracker.HasSceneBeenOpened(sceneIndex))
        {
            // Load the scene if it's unlocked
            Debug.Log("Button clicked, loading scene: " + sceneIndex);
            SceneManager.LoadScene(sceneIndex); // Make sure to have the correct scenes in your build settings
        }
        else
        {
            // Display message to the user if the scene is locked
            messageText.text = "Please complete the previous level first to unlock this one!";
            Debug.Log("Scene not unlocked, complete the previous level first.");
        }
    }

    // Get a color based on the intensity of t, using multiple colors (green, yellow, red)
    Color GetColorFromGradient(float t)
    {
        if (t < 0.5f)
        {
            // Green to Yellow
            return Color.Lerp(Color.green, Color.yellow, t * 2);
        }
        else
        {
            // Yellow to Red
            return Color.Lerp(Color.yellow, Color.red, (t - 0.5f) * 2);
        }
    }
}
