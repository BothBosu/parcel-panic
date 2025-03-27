using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Add this for scene management

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    [Header("Movement Settings")]
    public float playerSpeed = 2.0f;
    public float jumpHeight = 1.0f;
    public float gravityValue = -9.81f;
    public float rotationSpeed = 120f; // Higher value for faster rotation

    // Add a reference to a child object for camera targeting
    [Header("References")]
    public Transform cameraTarget;
    public Transform cameraTransform;

    // Modify impact settings to include level restart
    [Header("Impact Settings")]
    public float impactForce = 10f;
    public float recoveryTime = 1f;
    public bool restartLevelOnImpact = true; // New variable to control behavior
    public float restartDelay = 0.5f; // Time delay before restarting level
    public AudioClip crashSound; // Reference to the crash sound effect
    public float crashSoundVolume = 1.0f; // Volume for the crash sound
    private bool isRecoveringFromImpact = false;
    private Vector3 impactDirection;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction aimAction;

    // Add reference to the PlayerStateMachine
    [SerializeField] private PlayerStateMachine playerStateMachine;
    
    private void Start()
    { 
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        aimAction = playerInput.actions["Aim"];

        // Get reference to the PlayerStateMachine if not set
        if (playerStateMachine == null)
        {
            playerStateMachine = GetComponentInChildren<PlayerStateMachine>();
        }

        // Create camera target if it doesn't exist
        if (cameraTarget == null)
        {
            GameObject targetObj = new GameObject("CameraTarget");
            cameraTarget = targetObj.transform;
            cameraTarget.SetParent(transform);
            cameraTarget.localPosition = new Vector3(0, 1.5f, 0); // Adjust height as needed
        }

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Make your Cinemachine camera look at this target instead of the player
        // This step should be done in the Inspector
    }

    void Update()
    {
        if (isRecoveringFromImpact)
        {
            // Skip normal movement updates when recovering from impact
            HandleGravity();
            return;
        }

        // Handle gravity
        HandleGravity();

        // Handle player rotation based on mouse input
        Vector2 lookValue = lookAction.ReadValue<Vector2>();
        float mouseX = lookValue.x * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, mouseX);

        // Handle movement
        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        // Get camera-relative directions
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Project to horizontal plane
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction
        Vector3 moveDirection = forward * moveValue.y + right * moveValue.x;

        if (moveDirection.magnitude > 0.1f)
        {
            moveDirection.Normalize();
        }

        controller.Move(moveDirection * playerSpeed * Time.deltaTime);

        // Handle jumping
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
            
            // Trigger jump animation
            if (playerStateMachine != null)
            {
                playerStateMachine.TriggerJump();
            }
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    // Handle gravity in a separate method to avoid code duplication
    private void HandleGravity()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            
            // Inform the state machine that we're grounded (needed for animation transitions)
            if (playerStateMachine != null)
            {
                playerStateMachine.SetGroundedState(true);
            }
        }
        else if (!groundedPlayer)
        {
            // Inform the state machine that we're not grounded
            if (playerStateMachine != null)
            {
                playerStateMachine.SetGroundedState(false);
            }
        }
    }

    // Detect collisions with the vehicle
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if the collision is with a vehicle
        if (hit.gameObject.CompareTag("Vehicle") && !isRecoveringFromImpact)
        {
            // Get the vehicle's rigidbody
            Rigidbody vehicleRb = hit.gameObject.GetComponent<Rigidbody>();
            
            if (vehicleRb != null)
            {
                // Calculate impact direction (away from the vehicle)
                impactDirection = transform.position - hit.gameObject.transform.position;
                impactDirection.y = 0; // Keep it horizontal
                impactDirection.Normalize();
                
                // Calculate impact force based on vehicle velocity
                float impactMagnitude = vehicleRb.linearVelocity.magnitude * impactForce;
                
                if (restartLevelOnImpact)
                {
                    // Restart the level after a vehicle hit
                    StartCoroutine(RestartLevelAfterDelay(restartDelay));
                    
                    // Trigger impact animation and disable controls
                    HandleVehicleImpact();
                }
                else
                {
                    // Apply the original impact behavior
                    StartCoroutine(ApplyImpact(impactDirection, impactMagnitude));
                }
            }
        }
    }

    // New method to handle the vehicle impact when restarting level
    private void HandleVehicleImpact()
    {
        isRecoveringFromImpact = true;
        
        // Disable player input
        if (playerInput != null)
        {
            playerInput.actions.Disable();
        }
        
        // Trigger impact animation if available
        if (playerStateMachine != null && playerStateMachine.Animator != null)
        {
            playerStateMachine.Animator.SetTrigger("Impact");
        }
        
        // Play crash sound effect
        PlayCrashSound();
        
        // You could also add effects here like:
        // - Particle effects for the impact
        // - Screen shake
        // - Fade to black before restart
    }
    
    // Method to play crash sound effect
    private void PlayCrashSound()
    {
        if (crashSound != null)
        {
            // Play the sound at the player's position
            AudioSource.PlayClipAtPoint(crashSound, transform.position, crashSoundVolume);
        }
    }

    // New method to restart the level after a delay
    private IEnumerator RestartLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Restart the current scene
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private IEnumerator ApplyImpact(Vector3 direction, float force)
    {
        isRecoveringFromImpact = true;
        
        // Notify the state machine about the impact (if you want to trigger an animation)
        if (playerStateMachine != null)
        {
            // Assuming you have an Impact trigger in your animator
            // You might need to add this to your PlayerStateMachine class
            if (playerStateMachine.Animator != null)
            {
                playerStateMachine.Animator.SetTrigger("Impact");
            }
        }
        
        // Apply the impact force over several frames
        float elapsedTime = 0f;
        
        while (elapsedTime < recoveryTime)
        {
            // Add a slight upward component for a more natural looking impact
            Vector3 currentImpact = direction * force * (1 - (elapsedTime / recoveryTime));
            currentImpact.y = 0.5f * force * (1 - (elapsedTime / recoveryTime));
            
            // Move the character controller in the impact direction
            controller.Move(currentImpact * Time.deltaTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        isRecoveringFromImpact = false;
    }

    // Helper method to align with camera
    void AlignWithCamera()
    {
        // Get camera forward direction, but ignore Y axis
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        // Calculate the desired rotation
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        
        // Smoothly rotate the character to face the camera direction
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}