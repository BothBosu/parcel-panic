using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, Controls.IPlayerActions
{
    public Vector2 MovementValue { get; private set; }
    public bool IsRunning { get; private set; }
    public bool JumpPressed { get; private set; }

    // Track if pickup was just pressed this frame
    public bool JustPressedPickup { get; private set; }

    // Event for pickup action
    public event Action OnPickupEvent;

    private Controls controls;

    private void Start()
    {
        controls = new Controls();
        controls.Player.SetCallbacks(this);

        controls.Player.Enable();
    }

    private void Update()
    {
        // Reset the JustPressedPickup flag each frame
        JustPressedPickup = false;
    }

    private void OnDestroy()
    {
        controls.Player.Disable();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        // Track whether the run button is being held down
        if (context.performed)
        {
            IsRunning = true;
        }
        else if (context.canceled)
        {
            IsRunning = false;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
       
    }

    public void OnPickup(InputAction.CallbackContext context)
    {
        // Only trigger the event on button press (not release)
        if (context.performed)
        {
            // Set the flag that pickup was just pressed this frame
            JustPressedPickup = true;

            // Also invoke the event for compatibility with existing code
            OnPickupEvent?.Invoke();
        }
    }
}