using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ParcelLogic))]
public class ParcelDeliveryLogic : MonoBehaviour
{
    [Header("Delivery Settings")]
    [Tooltip("Optional particle effect to play when parcel is being carried to a valid destination")]
    [SerializeField] private ParticleSystem deliveryHintParticles;
    [SerializeField] private AudioClip deliverySound;

    [Header("Events")]
    public UnityEvent onDelivered;

    // Reference to the parcel's main logic component
    private ParcelLogic parcelLogic;

    // Track delivery status
    private bool isDelivered = false;

    private void Awake()
    {
        parcelLogic = GetComponent<ParcelLogic>();

        // Disable hint particles on start
        if (deliveryHintParticles != null)
        {
            deliveryHintParticles.Stop();
        }
    }

    // Called by the delivery destination when delivery is completed
    public void OnDelivered()
    {
        if (isDelivered) return; // Prevent double delivery

        isDelivered = true;

        /*
        // Play delivery sound if available
        if (deliverySound != null && AudioSource.PlayClipAtPoint != null)
        {
            AudioSource.PlayClipAtPoint(deliverySound, transform.position);
        }
        */

        // Stop any particles
        if (deliveryHintParticles != null)
        {
            deliveryHintParticles.Stop();
        }

        // Invoke the delivery event
        onDelivered?.Invoke();

        Debug.Log($"Parcel {gameObject.name} successfully delivered!");
    }

    // Helper method to set hint particles active/inactive
    public void SetHintParticles(bool active)
    {
        if (deliveryHintParticles == null) return;

        if (active)
        {
            if (!deliveryHintParticles.isPlaying)
            {
                deliveryHintParticles.Play();
            }
        }
        else
        {
            if (deliveryHintParticles.isPlaying)
            {
                deliveryHintParticles.Stop();
            }
        }
    }
}