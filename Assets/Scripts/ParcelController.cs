using UnityEngine;

public class ParcelController : MonoBehaviour
{
    [Header("Parcel Settings")]
    public float mass = 1f;
    public ParticleSystem collectEffect; // Optional particle effect on collection

    [HideInInspector]
    public float lastThrownTime = 0f;

    private void OnTriggerEnter(Collider other) {
        // Check if the collision is with the player
        if (other.CompareTag("Player"))
        {
            // IMPORTANT: Add this to prevent re-collecting immediately after throwing
            // Get the player's parcel manager
            PlayerParcelManager parcelManager = other.GetComponent<PlayerParcelManager>();
            
            // Only proceed if enough time has passed since being thrown
            if (Time.time < lastThrownTime + 0.5f)
            {
                return; // Ignore collection attempts right after throwing
            }
            
            if (parcelManager != null && parcelManager.CollectParcel(this))
            {
                // Play collection effect if assigned
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, Quaternion.identity);
                }
                
                // Deactivate the parcel object
                gameObject.SetActive(false);
                // GetComponent<Collider>().enabled = false;
            }
        }
    }
}
