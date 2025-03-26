using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputReader : MonoBehaviour, Controls.IPlayerActions
{
    public Vector2 MovementValue { get; private set; }
    public bool IsRunning { get; private set; }
    public bool JumpPressed { get; private set; }

    // Track if pickup was just pressed this frame
    public bool JustPressedPickup { get; private set; }

    // Track if throw button is being held down
    public bool IsThrowButtonHeld { get; private set; }

    // Track if throw button was just released this frame
    public bool JustReleasedThrow { get; private set; }

    // Event for pickup action
    public event Action OnPickupEvent;

    // Event for throw action
    public event Action OnThrowStartEvent;
    public event Action OnThrowReleaseEvent;

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
    public void OnThrow(InputAction.CallbackContext context)
    {
        // When throw button is pressed
        if (context.performed)
        {
            IsThrowButtonHeld = true;
            OnThrowStartEvent?.Invoke();
        }
        // When throw button is released
        else if (context.canceled)
        {
            IsThrowButtonHeld = false;
            JustReleasedThrow = true;
            OnThrowReleaseEvent?.Invoke();
        }
    }
}