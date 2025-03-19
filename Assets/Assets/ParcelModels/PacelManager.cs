using System.Collections.Generic;
using UnityEngine;

public class ParcelManager : MonoBehaviour
{
    private static ParcelManager _instance;
    public static ParcelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance
                _instance = FindObjectOfType<ParcelManager>();

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

    // Reference to player
    private PlayerStateMachine playerStateMachine;
    private InputReader inputReader;

    private void Awake()
    {
        // Ensure we have only one instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Find player references
        playerStateMachine = FindObjectOfType<PlayerStateMachine>();
        if (playerStateMachine != null)
        {
            inputReader = playerStateMachine.InputReader;
            inputReader.OnPickupEvent += HandlePickupInput;
        }
        else
        {
            Debug.LogError("Cannot find PlayerStateMachine in the scene!");
        }
    }

    private void OnDestroy()
    {
        if (inputReader != null)
        {
            inputReader.OnPickupEvent -= HandlePickupInput;
        }
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
            return;
        }

        // Otherwise, try to pick up the closest valid parcel
        ParcelLogic closestParcel = FindClosestPickableParcel();

        if (closestParcel != null)
        {
            // Remember which parcel we're carrying
            carriedParcel = closestParcel;

            // Tell the parcel to handle pickup
            closestParcel.HandlePickup(GetCurrentPlayerState());
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