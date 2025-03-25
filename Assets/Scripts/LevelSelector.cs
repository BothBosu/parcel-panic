using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    // Method to load a specific level
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    // Method to load the Main Menu scene
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");  // Replace with your actual Main Menu scene name
    }
}
