using UnityEngine;

public class ParcelController : MonoBehaviour
{
    [Header("Parcel Settings")]
    public float mass = 1f;
    public ParticleSystem collectEffect; // Optional particle effect on collection

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collision is with the player
        if (other.CompareTag("Player"))
        {
            // Find the player's parcel manager and try to collect this parcel
            PlayerParcelManager parcelManager = other.GetComponent<PlayerParcelManager>();
            if (parcelManager != null && parcelManager.CollectParcel(this))
            {
                // Play collection effect if assigned
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, Quaternion.identity);
                }
                
                // Deactivate the parcel object - can be pooled instead of destroyed for optimization
                gameObject.SetActive(false);
            }
        }
    }
}