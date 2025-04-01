using UnityEngine;
using UnityEngine.UI;

public class FirstTimePopup : MonoBehaviour
{
    [Header("Popup Panel")]
    [SerializeField] private GameObject popupPanel;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    [Header("Dependencies")]
    [SerializeField] private CountdownTimer timerController;

    void Start()
    {
        // Activate popup and pause the game
        popupPanel.SetActive(true);
        PauseGame();

        // Add listener to close button
        closeButton.onClick.AddListener(ClosePopup);
    }

    private void PauseGame()
    {
        // Pause timer if exists
        if (timerController != null)
            timerController.ToggleTimer(false);

        // Pause the game time
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        // Resume timer if exists
        if (timerController != null)
            timerController.ToggleTimer(true);

        // Resume the game time
        Time.timeScale = 1f;
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
        ResumeGame();
    }
}
