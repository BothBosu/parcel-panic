using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Button")]
    [SerializeField] private Button pauseButton;
    
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Scene References")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    
    [Header("Dependencies")]
    [SerializeField] private CountdownTimer timerController;
    
    private bool isPaused = false;
    
    private void Start()
    {
        // Hide pause menu initially
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        // Set up button listeners
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);
        
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ContinueGame();
        }
    }
    
    private void PauseGame()
    {
        // Pause the timer
        if (timerController != null)
            timerController.ToggleTimer(false);
        
        // Show pause menu
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
        
        // Optional: Pause the game time
        Time.timeScale = 0f;
    }
    
    public void ContinueGame()
    {
        // Resume the timer
        if (timerController != null)
            timerController.ToggleTimer(true);
        
        // Hide pause menu
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        // Resume game time
        Time.timeScale = 1f;
        
        isPaused = false;
    }
    
    public void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1f;
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void GoToMainMenu()
    {
        // Reset time scale
        Time.timeScale = 1f;
        
        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}