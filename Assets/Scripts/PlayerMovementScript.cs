using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class HumanoidPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Walking speed in units per second")]
    public float walkSpeed = 2.0f;

    [Tooltip("Running speed in units per second")]
    public float runSpeed = 6.0f;

    [Tooltip("How fast the player turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float rotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float speedChangeRate = 10.0f;

    [Header("Jump Settings")]
    [Tooltip("The height the player can jump")]
    public float jumpHeight = 1.2f;

    [Tooltip("Time required to pass before being able to jump again")]
    public float jumpCooldown = 0.5f;

    [Tooltip("How fast the character falls when not grounded")]
    public float fallSpeed = 2.0f;

    [Header("Ground Settings")]
    [Tooltip("If the character is grounded or not")]
    public bool grounded = true;

    [Tooltip("Useful for rough ground")]
    public float groundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check")]
    public float groundedRadius = 0.28f;

    [Tooltip("What layers are considered ground for the character")]
    public LayerMask groundLayers;

    [Header("Animation Settings")]
    [Tooltip("Reference to the animator component")]
    public Animator animator;

    // Player variables
    private CharacterController controller;
    private GameObject mainCamera;
    private float speed;
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float verticalVelocity;
    private float terminalVelocity = -53.0f;
    private bool canJump = true;
    private bool hasAnimator;

    // Animation IDs
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;

    private void Awake()
    {
        // Get reference to our main camera
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        hasAnimator = animator != null;
        controller = GetComponent<CharacterController>();

        // Initialize animation IDs
        AssignAnimationIDs();
    }

    private void Update()
    {
        GroundedCheck();
        Move();
        JumpAndGravity();
        UpdateAnimator();
    }

    private void AssignAnimationIDs()
    {
        if (!hasAnimator) return;

        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        // Set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        // Update animator if using character
        if (hasAnimator)
        {
            animator.SetBool(animIDGrounded, grounded);
        }
    }

    private void Move()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Calculate input direction
        Vector3 inputDirection = new Vector3(horizontal, 0.0f, vertical).normalized;

        // Target speed based on whether running or walking
        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        // If no input, set target speed to 0
        if (inputDirection == Vector3.zero) targetSpeed = 0.0f;

        // Get current horizontal velocity
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

        // Acceleration and deceleration smoothing
        float speedOffset = 0.1f;
        float inputMagnitude = inputDirection.magnitude;

        // Accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // Creates curved result rather than linear
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);

            // Round speed to 3 decimal places
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
        }

        // Animation blend for smoother transitions
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        if (animationBlend < 0.01f) animationBlend = 0f;

        // Normalize input direction
        Vector3 targetDirection = inputDirection;

        // Handle rotation with camera direction considered
        if (inputDirection != Vector3.zero)
        {
            // Get the camera's forward and right directions
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;

            // Project onto the horizontal plane
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // Calculate target direction relative to camera orientation
            targetDirection = camForward * inputDirection.z + camRight * inputDirection.x;

            // Calculate target rotation to face movement direction
            targetRotation = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

            // Smooth rotation
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);

            // Rotate to face input direction relative to camera
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        // Move the player
        Vector3 targetMovement = targetDirection * speed;

        // Add vertical velocity (gravity/jump)
        targetMovement.y = verticalVelocity;

        // Apply movement
        controller.Move(targetMovement * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (grounded)
        {
            // Reset the fall timeout timer
            verticalVelocity = -2f; // Small downward force to keep grounded

            // Jump
            if (Input.GetKeyDown(KeyCode.Space) && canJump)
            {
                // Calculate jump velocity based on height
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);

                // Trigger jump animation
                if (hasAnimator)
                {
                    animator.SetBool(animIDJump, true);
                }

                // Set cooldown
                StartCoroutine(JumpCooldown());
            }
            else
            {
                // Reset jump animation
                if (hasAnimator)
                {
                    animator.SetBool(animIDJump, false);
                }
            }
        }
        else
        {
            // Apply gravity over time if not grounded
            verticalVelocity += Physics.gravity.y * fallSpeed * Time.deltaTime;

            // Clamp to terminal velocity
            verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);

            // Update animator
            if (hasAnimator)
            {
                // Set the free fall animation
                animator.SetBool(animIDFreeFall, verticalVelocity < -1.5f);
            }
        }
    }

    private IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    private void UpdateAnimator()
    {
        if (!hasAnimator) return;

        // Update the animator parameters
        animator.SetFloat(animIDSpeed, animationBlend);
        animator.SetFloat(animIDMotionSpeed, inputMagnitude);
    }

    private float inputMagnitude
    {
        get
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return Mathf.Clamp01(new Vector3(horizontal, 0.0f, vertical).magnitude);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a sphere at the ground check position
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        // Grounded check sphere
        Gizmos.color = grounded ? transparentGreen : transparentRed;

        // Draw sphere on selected object only
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
            groundedRadius);
    }
}