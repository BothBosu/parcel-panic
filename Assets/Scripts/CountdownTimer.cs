using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float totalTime = 60f; // Total time in seconds
    [SerializeField] private TMPro.TextMeshProUGUI timerText; // Reference to UI Text component
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip tickingSound; // Reference to the ticking sound effect
    [SerializeField] private float tickingSoundThreshold = 10f; // Time threshold when ticking starts (in seconds)
    [SerializeField] private float tickingVolume = 1f; // Volume of the ticking sound
    [SerializeField] private bool loopTickingSound = true; // Whether the sound should loop
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel; // Reference to game over panel
    [SerializeField] private Button restartButton; // Reference to restart button
    [SerializeField] private Button mainMenuButton; // Reference to main menu button
    
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Name of the main menu scene
    
    [Header("Player References")]
    [SerializeField] private GameObject player; // Reference to player GameObject
    
    private float timeRemaining;
    private bool isTimerRunning = false;
    private bool isTickingSoundPlaying = false;
    private AudioSource audioSource;
    
    void Start()
    {
        // Initialize timer
        timeRemaining = totalTime;
        isTimerRunning = true;
        
        // Hide game over panel initially
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Add button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        // Set up audio source for ticking sound
        SetupAudioSource();
        
        // Update the timer display
        UpdateTimerDisplay();
    }
    
    void SetupAudioSource()
    {
        // Get existing AudioSource or add a new one if none exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure the audio source
        audioSource.playOnAwake = false;
        audioSource.loop = loopTickingSound;
        audioSource.volume = tickingVolume;
        audioSource.clip = tickingSound;
    }
    
    void Update()
    {
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                // Decrease timer
                timeRemaining -= Time.deltaTime;
                
                // Check if we should play ticking sound
                CheckTickingSound();
                
                UpdateTimerDisplay();
            }
            else
            {
                // Timer has reached zero
                timeRemaining = 0;
                isTimerRunning = false;
                StopTickingSound();
                UpdateTimerDisplay(); // Update one last time to ensure "0" is displayed
                GameOver();
            }
        }
    }
    
    void CheckTickingSound()
    {
        // Start playing ticking sound when timer drops below threshold
        if (timeRemaining <= tickingSoundThreshold && !isTickingSoundPlaying)
        {
            PlayTickingSound();
        }
        // Stop ticking sound if timer goes back above threshold (e.g., after adding time)
        else if (timeRemaining > tickingSoundThreshold && isTickingSoundPlaying)
        {
            StopTickingSound();
        }
    }
    
    void PlayTickingSound()
    {
        if (tickingSound != null && audioSource != null)
        {
            audioSource.Play();
            isTickingSoundPlaying = true;
            Debug.Log("Ticking sound started");
        }
    }
    
    void StopTickingSound()
    {
        if (audioSource != null && isTickingSoundPlaying)
        {
            audioSource.Stop();
            isTickingSoundPlaying = false;
            Debug.Log("Ticking sound stopped");
        }
    }
    
    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Ensure time doesn't go negative
            float timeToDisplay = Mathf.Max(0, timeRemaining);
            
            // Format time to display only seconds
            int seconds = Mathf.CeilToInt(timeToDisplay);
            
            // Update UI text with just the seconds
            timerText.text = seconds.ToString();
        }
    }
    
    void GameOver()
    {
        Debug.Log("Game Over!");
        
        // Stop ticking sound if it's playing
        StopTickingSound();
        
        // Pause the game time - this will stop all movement
        Time.timeScale = 0f;
        
        // Show game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        // Disable player movement
        DisablePlayerMovement();
    }
    
    void DisablePlayerMovement()
    {
        if (player != null)
        {
            // Disable the PlayerController script
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            
            // Disable PlayerInput to prevent any input actions
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }
            
            // Optionally freeze the CharacterController
            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            
            // Disable any Rigidbody if present
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            
            // If using 2D physics
            Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.simulated = false;
            }
            
            Debug.Log("Player movement disabled");
        }
        else
        {
            Debug.LogWarning("Player reference not set in CountdownTimer script!");
        }
    }
    
    public void RestartGame()
    {
        // Reset time scale before loading new scene
        Time.timeScale = 1f;
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void GoToMainMenu()
    {
        // Reset time scale before loading new scene
        Time.timeScale = 1f;
        
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    // Method to add time (could be used for pickups or bonuses)
    public void AddTime(float timeToAdd)
    {
        timeRemaining += timeToAdd;
        
        // Check if we need to stop the ticking sound after adding time
        if (timeRemaining > tickingSoundThreshold && isTickingSoundPlaying)
        {
            StopTickingSound();
        }
    }
    
    // Method to pause/resume the timer
    public void ToggleTimer(bool running)
    {
        isTimerRunning = running;
        
        // Pause/resume ticking sound based on timer state
        if (!running && isTickingSoundPlaying)
        {
            audioSource.Pause();
        }
        else if (running && isTickingSoundPlaying)
        {
            audioSource.UnPause();
        }
    }
}