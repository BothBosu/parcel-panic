using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ParcelManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject pickupPromptUI;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float uiUpdateInterval = 0.1f;

    private static ParcelManager _instance;
    public static ParcelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance
                _instance = FindFirstObjectByType<ParcelManager>();

                // Create new instance if none exists
                if (_instance == null)
                {
                    GameObject managerObj = new GameObject("ParcelManager");
                    _instance = managerObj.AddComponent<ParcelManager>();
                }
            }
            return _instance;
        }
    }

    // List of all parcels in the scene
    private List<ParcelLogic> parcels = new List<ParcelLogic>();

    // Currently carried parcel
    private ParcelLogic carriedParcel = null;

    // Closest pickable parcel (for UI display)
    private ParcelLogic closestPickableParcel = null;

    // Reference to player
    private PlayerStateMachine playerStateMachine;
    private InputReader inputReader;

    // Timer for UI updates
    private float uiUpdateTimer = 0f;

    private void Awake()
    {
        // Ensure we have only one instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // Subscribe to scene change events instead of using DontDestroyOnLoad
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Find player references
        playerStateMachine = FindFirstObjectByType<PlayerStateMachine>();
        if (playerStateMachine != null)
        {
            inputReader = playerStateMachine.InputReader;

            // Subscribe to input events
            inputReader.OnPickupEvent += HandlePickupInput;
            inputReader.OnThrowStartEvent += HandleThrowStartInput;
        }
        else
        {
            Debug.LogError("Cannot find PlayerStateMachine in the scene!");
        }

        // Hide UI at start
        if (pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Update pickup UI at intervals to avoid doing it every frame
        uiUpdateTimer += Time.deltaTime;
        if (uiUpdateTimer >= uiUpdateInterval)
        {
            UpdatePickupUI();
            uiUpdateTimer = 0f;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene events
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        if (inputReader != null)
        {
            inputReader.OnPickupEvent -= HandlePickupInput;
            inputReader.OnThrowStartEvent -= HandleThrowStartInput;
        }

        // Clear static instance reference if this is the current instance
        if (_instance == this)
        {
            _instance = null;
        }
    }

    // Handle scene unloading
    private void OnSceneUnloaded(Scene scene)
    {
        // Clear static instance reference
        if (_instance == this)
        {
            // Ensure we destroy the GameObject properly
            Destroy(gameObject);
            _instance = null;
        }
    }

    // Clean up when application quits
    private void OnApplicationQuit()
    {
        _instance = null;
    }

    // Register a parcel with the manager
    public void RegisterParcel(ParcelLogic parcel)
    {
        if (!parcels.Contains(parcel))
        {
            parcels.Add(parcel);
        }
    }

    // Unregister a parcel from the manager
    public void UnregisterParcel(ParcelLogic parcel)
    {
        parcels.Remove(parcel);

        // If this was the carried parcel, clear the reference
        if (carriedParcel == parcel)
        {
            carriedParcel = null;
        }

        // If this was the closest pickable parcel, update the UI
        if (closestPickableParcel == parcel)
        {
            closestPickableParcel = null;
            UpdatePickupUI();
        }
    }

    // Central handler for pickup input
    private void HandlePickupInput()
    {
        // If we're carrying a parcel, drop it
        if (carriedParcel != null)
        {
            // Get current state before dropping
            PlayerBaseState currentState = GetCurrentPlayerState();

            // Tell the parcel to drop itself
            carriedParcel.Drop(playerStateMachine.transform.forward);
            carriedParcel = null;

            // Switch back to normal state
            if (currentState != null)
            {
                playerStateMachine.SwitchState(currentState);
            }

            // Update the UI after dropping
            UpdatePickupUI();
            return;
        }

        // Otherwise, try to pick up the closest valid parcel
        ParcelLogic parcelToPickup = closestPickableParcel ?? FindClosestPickableParcel();

        if (parcelToPickup != null)
        {
            // Remember which parcel we're carrying
            carriedParcel = parcelToPickup;

            // Tell the parcel to handle pickup
            parcelToPickup.HandlePickup(GetCurrentPlayerState());

            // Hide the UI while carrying
            if (pickupPromptUI != null)
            {
                pickupPromptUI.SetActive(false);
            }

            // Clear closest pickable reference
            closestPickableParcel = null;
        }
    }

    // New method to handle throw start input
    private void HandleThrowStartInput()
    {
        // Only allow throwing if we're currently carrying a parcel
        if (carriedParcel != null)
        {
            // Get current state before entering throwing state
            PlayerBaseState currentState = GetCurrentPlayerState();

            // Create the throwing state
            PlayerThrowingState throwingState = new PlayerThrowingState(
                playerStateMachine,
                carriedParcel.transform,
                currentState
            );

            // Switch to throwing state
            playerStateMachine.SwitchState(throwingState);

            // Hide the UI while in throwing state
            if (pickupPromptUI != null)
            {
                pickupPromptUI.SetActive(false);
            }
        }
    }

    // Updates the pickup UI based on nearby pickable parcels
    private void UpdatePickupUI()
    {
        // Skip if no UI assigned
        if (pickupPromptUI == null) return;

        // If we're carrying something, hide the UI
        if (carriedParcel != null)
        {
            pickupPromptUI.SetActive(false);
            closestPickableParcel = null;
            return;
        }

        // Find the closest pickable parcel
        closestPickableParcel = FindClosestPickableParcel();

        // Show/hide UI based on whether there's a pickable parcel
        if (closestPickableParcel != null)
        {
            // Show the UI and position it above the parcel
            pickupPromptUI.SetActive(true);

            // Get the position to show the UI (above the parcel)
            Vector3 uiPosition = closestPickableParcel.GetPickupTargetPosition() + uiOffset;
            pickupPromptUI.transform.position = uiPosition;

            // Make UI face the camera if there's a main camera
            if (Camera.main != null)
            {
                pickupPromptUI.transform.rotation = Camera.main.transform.rotation;
            }
        }
        else
        {
            // No pickable parcel nearby, hide the UI
            pickupPromptUI.SetActive(false);
        }
    }

    // Find the closest parcel that can be picked up
    private ParcelLogic FindClosestPickableParcel()
    {
        if (playerStateMachine == null) return null;

        Vector3 playerPosition = playerStateMachine.transform.position;
        Vector3 playerEyePosition = playerPosition + Vector3.up * 1.6f;

        ParcelLogic closest = null;
        float closestDistance = float.MaxValue;

        foreach (ParcelLogic parcel in parcels)
        {
            // Skip if already being carried
            if (parcel.IsPickedUp) continue;

            // Check distance
            float distance = Vector3.Distance(playerPosition, parcel.transform.position);
            if (distance > parcel.PickupDistance) continue;

            // Check line of sight
            Vector3 parcelPosition = parcel.GetPickupTargetPosition();
            Vector3 directionToParcel = (parcelPosition - playerEyePosition).normalized;

            RaycastHit hit;

            if (Physics.Raycast(playerEyePosition, directionToParcel, out hit, parcel.PickupDistance))
            {
                // If we hit the parcel
                if (hit.collider.gameObject == parcel.gameObject)
                {
                    // Check if this is closer than our current closest
                    if (distance < closestDistance)
                    {
                        closest = parcel;
                        closestDistance = distance;
                    }
                }
            }
        }

        return closest;
    }

    // Helper to get the current player state
    private PlayerBaseState GetCurrentPlayerState()
    {
        // Create appropriate state based on current player state
        if (playerStateMachine.InputReader.IsRunning)
        {
            return new PlayerRunningState(playerStateMachine);
        }
        else
        {
            return new PlayerTestState(playerStateMachine);
        }
    }
}