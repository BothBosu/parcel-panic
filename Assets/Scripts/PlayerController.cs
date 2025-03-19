using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public Animator animator;

    [Header("Knockdown Settings")]
    public float knockdownDuration = 3.0f;
    public float knockbackForce = 10.0f;
    public float minImpactForce = 3.0f;
    private bool isKnockedDown = false;
    private Rigidbody rb;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction aimAction;

    private void Start()
    { 
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        aimAction = playerInput.actions["Aim"];
        
        // Get or add Rigidbody for knockback physics
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Let CharacterController handle normal movement
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        
        // Get animator if available
        if (animator == null)
        {
            animator = GetComponent<Animator>();
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
        // Skip normal movement processing if knocked down
        if (isKnockedDown)
            return;
            
        // Handle gravity
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        
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
        }
        
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
    
    void OnCollisionEnter(Collision collision)
    {
    // Check if collision is with a vehicle
    if (collision.gameObject.CompareTag("Vehicle") && !isKnockedDown)
    {
        float impactForce = collision.relativeVelocity.magnitude;
        
        if (impactForce > minImpactForce)
        {
            // Get the contact point and normal for better direction calculation
            ContactPoint contact = collision.contacts[0];
            
            // Get car's forward direction (assuming Z-axis is forward)
            Vector3 carForward = collision.transform.forward;
            
            // Get perpendicular vector to car's forward direction (this is the side direction)
            Vector3 carSide = Vector3.Cross(Vector3.up, carForward).normalized;
            
            // Determine which side of the car the player is on
            Vector3 playerToCar = collision.transform.position - transform.position;
            float dotProduct = Vector3.Dot(playerToCar, carSide);
            
            // If dot product is positive, player is on the left, negative means right
            Vector3 knockbackDirection = dotProduct > 0 ? -carSide : carSide;
            
            // Add a slight backward component to avoid sliding along the car
            knockbackDirection += -carForward * 0.5f;
            knockbackDirection.Normalize();
            
            // Keep direction horizontal
            knockbackDirection.y = 0;
            
            // Apply knockback with the corrected direction
            ApplyKnockback(knockbackDirection, impactForce);
        }
    }
    }
    
    public void ApplyKnockback(Vector3 direction, float intensity)
{
    if (isKnockedDown)
        return;
        
    // Start knockdown
    isKnockedDown = true;
    
    // Disable character controller
    controller.enabled = false;
    
    // Enable rigidbody physics
    rb.isKinematic = false;
    
    // Add upward force to prevent the player from sliding on the ground
    Vector3 knockbackVector = direction * knockbackForce * intensity;
    knockbackVector.y = 3.0f; // Add a slight upward force
    
    // Apply force
    rb.AddForce(knockbackVector, ForceMode.Impulse);
    
    // Trigger animation
    if (animator != null)
        animator.SetTrigger("KnockDown");
    
    // Start recovery timer
    StartCoroutine(RecoverFromKnockdown());
    }
    
    private IEnumerator RecoverFromKnockdown()
    {
        yield return new WaitForSeconds(knockdownDuration);
        
        // Play recovery animation
        if (animator != null)
        {
            animator.SetTrigger("GetUp");
            
            // Wait for animation to complete
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length * 0.9f);
        }
        
        // Reset player position to be upright
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        
        // Re-enable character controller
        rb.isKinematic = true;
        controller.enabled = true;
        
        // Reset state
        isKnockedDown = false;
        playerVelocity = Vector3.zero;
    }
}