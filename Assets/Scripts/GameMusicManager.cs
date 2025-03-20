using UnityEngine;

public class GameMusicManager : MonoBehaviour
{
    // Singleton instance
    private static GameMusicManager instance;

    // Audio source component reference
    private AudioSource musicSource;

    void Awake()
    {
        // Singleton pattern to make sure we only have one music manager
        if (instance == null)
        {
            // This is the first instance - make it persistent
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Get the audio source component
            musicSource = GetComponent<AudioSource>();
            
            // Make sure it's playing
            if (musicSource != null && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
        else
        {
            // Another instance already exists - destroy this one
            Destroy(gameObject);
        }
    }
}