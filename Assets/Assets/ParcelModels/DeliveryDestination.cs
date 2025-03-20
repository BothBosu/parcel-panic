using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeliveryDestination : MonoBehaviour
{
    [Header("Delivery Settings")]
    [Tooltip("How close the player needs to be to deliver a parcel")]
    [SerializeField] private float deliveryRange = 2.5f;
    [SerializeField] private List<string> acceptedParcelTags = new List<string>();
    [SerializeField] private ParticleSystem deliverySuccessParticles;

    [Header("UI Settings")]
    [SerializeField] private GameObject deliveryPromptUI;
    [SerializeField] private GameObject deliverySuccessUI;
    [SerializeField] private float successMessageDuration = 1.5f;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 1.5f, 0);

    // Reference to player and carried parcel
    private PlayerStateMachine playerStateMachine;
    private InputReader inputReader;
    private ParcelLogic carriedParcel;

    // Check for delivery eligibility
    private bool canDeliver = false;

    // Timer variable to update UI
    private float updateTimer = 0f;
    private float updateInterval = 0.1f;

    private void Start()
    {
        // Find the player and input reader
        playerStateMachine = FindFirstObjectByType<PlayerStateMachine>();

        if (playerStateMachine != null)
        {
            inputReader = playerStateMachine.InputReader;

            // Subscribe to the same pickup button for delivery action
            inputReader.OnPickupEvent += HandleDeliveryInput;
        }
        else
        {
            Debug.LogError("Cannot find PlayerStateMachine in the scene!");
        }

        // Hide UIs at start
        if (deliveryPromptUI != null)
        {
            deliveryPromptUI.SetActive(false);
        }

        if (deliverySuccessUI != null)
        {
            deliverySuccessUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Update UI and check for delivery eligibility at intervals
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            CheckForDelivery();
            updateTimer = 0f;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from input events
        if (inputReader != null)
        {
            inputReader.OnPickupEvent -= HandleDeliveryInput;
        }
    }

    // Check if the player is carrying a valid parcel within delivery range
    private void CheckForDelivery()
    {
        // Reset delivery status
        canDeliver = false;

        // Make sure we have the player reference
        if (playerStateMachine == null) return;

        // Get player position
        Vector3 playerPosition = playerStateMachine.transform.position;

        // Check if player is in range of the delivery point
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);

        if (distanceToPlayer > deliveryRange)
        {
            // Player too far away
            UpdateDeliveryUI(false);
            return;
        }

        // Check if the player is carrying a parcel
        ParcelManager parcelManager = ParcelManager.Instance;
        if (parcelManager == null)
        {
            Debug.LogWarning("Cannot find ParcelManager!");
            UpdateDeliveryUI(false);
            return;
        }

        // Find any carried parcel
        carriedParcel = FindCarriedParcel();

        // No parcel being carried
        if (carriedParcel == null)
        {
            UpdateDeliveryUI(false);
            return;
        }

        // Check if this parcel is deliverable (has ParcelDeliveryLogic component)
        ParcelDeliveryLogic deliveryLogic = carriedParcel.GetComponent<ParcelDeliveryLogic>();
        if (deliveryLogic == null)
        {
            // This parcel is not deliverable
            UpdateDeliveryUI(false);
            return;
        }

        // Check if this destination accepts this parcel
        if (CanAcceptParcel(carriedParcel.gameObject))
        {
            // Valid parcel in range!
            canDeliver = true;
            UpdateDeliveryUI(true);
        }
        else
        {
            // Wrong parcel type
            UpdateDeliveryUI(false);
        }
    }

    // Try to find the parcel currently being carried by the player
    private ParcelLogic FindCarriedParcel()
    {
        // Find all parcels in the scene
        ParcelLogic[] allParcels = FindObjectsOfType<ParcelLogic>();

        foreach (ParcelLogic parcel in allParcels)
        {
            // Return the first parcel that's being carried
            if (parcel.IsPickedUp)
            {
                return parcel;
            }
        }

        // No carried parcel found
        return null;
    }

    // Update the delivery UI based on delivery eligibility
    private void UpdateDeliveryUI(bool showPrompt)
    {
        // Skip if no UI components
        if (deliveryPromptUI == null) return;

        // Only update if the success UI isn't already showing
        if (deliverySuccessUI != null && deliverySuccessUI.activeSelf)
        {
            deliveryPromptUI.SetActive(false);
            return;
        }

        // Show/hide the delivery prompt
        deliveryPromptUI.SetActive(showPrompt);

        if (showPrompt)
        {
            // Position the UI above the destination
            Vector3 uiPosition = transform.position + uiOffset;
            deliveryPromptUI.transform.position = uiPosition;

            // Make UI face the camera
            if (Camera.main != null)
            {
                deliveryPromptUI.transform.rotation = Camera.main.transform.rotation;
            }
        }
    }

    // Check if this destination accepts a particular parcel
    public bool CanAcceptParcel(GameObject parcel)
    {
        // If no tags specified, accept all parcels
        if (acceptedParcelTags.Count == 0)
        {
            return true;
        }

        // Otherwise, check if the parcel has any of the accepted tags
        foreach (string tag in acceptedParcelTags)
        {
            if (parcel.CompareTag(tag))
            {
                return true;
            }
        }

        // No matching tags found
        return false;
    }

    // Handle the delivery button press
    private void HandleDeliveryInput()
    {
        // Only process if we can deliver
        if (!canDeliver || carriedParcel == null) return;

        // Complete the delivery
        ParcelDeliveryLogic deliveryLogic = carriedParcel.GetComponent<ParcelDeliveryLogic>();
        if (deliveryLogic != null)
        {
            // Notify the parcel it's been delivered (allows for custom delivery effects/sounds)
            deliveryLogic.OnDelivered();
            CompleteDelivery(carriedParcel);
        }
    }

    // Process a successful delivery
    private void CompleteDelivery(ParcelLogic parcel)
    {
        // Play success particles if available
        if (deliverySuccessParticles != null)
        {
            deliverySuccessParticles.Play();
        }

        // Hide the prompt UI
        if (deliveryPromptUI != null)
        {
            deliveryPromptUI.SetActive(false);
        }

        // Show success UI if available
        if (deliverySuccessUI != null)
        {
            // Position the success UI at the same position as the prompt
            deliverySuccessUI.transform.position = transform.position + uiOffset;

            // Make UI face the camera
            if (Camera.main != null)
            {
                deliverySuccessUI.transform.rotation = Camera.main.transform.rotation;
            }

            // Show the success UI
            deliverySuccessUI.SetActive(true);

            // Start coroutine to hide the success UI after a delay
            StartCoroutine(HideSuccessUIAfterDelay(successMessageDuration));
        }

        // Reset delivery status
        canDeliver = false;

        // Cache the parcel reference before nulling it
        ParcelLogic deliveredParcel = carriedParcel;
        carriedParcel = null;

        // Reset player state to normal
        playerStateMachine.SwitchState(new PlayerTestState(playerStateMachine));

        // Destroy the parcel
        Destroy(deliveredParcel.gameObject);

        Debug.Log("Delivery completed successfully!");
    }

    // Coroutine to hide the success UI after a delay
    private System.Collections.IEnumerator HideSuccessUIAfterDelay(float delay)
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(delay);

        // Hide the success UI
        if (deliverySuccessUI != null)
        {
            deliverySuccessUI.SetActive(false);
        }
    }

    // Visualize the delivery range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, deliveryRange);
    }
}