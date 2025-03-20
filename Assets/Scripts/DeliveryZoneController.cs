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
                // If there's no sound, just load the scene
                SceneManager.LoadScene("MainMenuScene");
            }
        }
    }
    
    private System.Collections.IEnumerator LoadNextSceneAfterSound()
    {
        // Wait for the sound to finish playing
        float waitTime = victorySound.length;
        yield return new WaitForSeconds(waitTime);
        
        // Load the next scene
        SceneManager.LoadScene("MainMenuScene");
    }
}