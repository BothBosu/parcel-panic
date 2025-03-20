using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public Button startButton;
    public Button selectLevelButton;
    public Button exitButton;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        startButton = root.Q<Button>("start-button");
        selectLevelButton = root.Q<Button>("selectLevel-button");
        exitButton = root.Q<Button>("exit-button");
        
        startButton.clicked += StartButtonPressed;
        selectLevelButton.clicked += SelectLevelButtonPressed;
        exitButton.clicked += ExitButtonPressed;
    }

    void StartButtonPressed()
    {
        SceneManager.LoadScene("PrototypeScene");
    }
    
    void SelectLevelButtonPressed()
    {
        SceneManager.LoadScene("SelectLevelScene");
    }

    void ExitButtonPressed()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}