using UnityEngine;
using UnityEngine.SceneManagement;

public class DeliveryZoneController : MonoBehaviour
{
    [SerializeField] private AudioClip victorySound; // Reference to your victory sound
    private AudioSource audioSource;

    private void Start()
    {
        // Add an AudioSource component if one doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ParcelController parcel = other.GetComponent<ParcelController>();

        if (parcel != null)
        {
            // Play victory sound
            if (victorySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(victorySound);
                
                // Wait for the sound to finish before loading the next scene
                StartCoroutine(LoadNextSceneAfterSound());
            }
            else
            {
                // If there's no sound, just load the next scene
                LoadNextScene();
            }
        }
    }
    
    private System.Collections.IEnumerator LoadNextSceneAfterSound()
    {
        // Wait for the sound to finish playing
        float waitTime = victorySound.length;
        yield return new WaitForSeconds(waitTime);
        
        // Load the next scene
        LoadNextScene();
    }
    
    private void LoadNextScene()
    {
        // Calculate the next scene's index
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // Ensure the next scene exists in the Build Settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Optionally handle what happens if there is no next scene.
            // For example, loop back to the first scene or show an end-of-game screen.
            Debug.Log("No more levels to load. Restarting at level 0.");
            SceneManager.LoadScene(0);
        }
    }
}
