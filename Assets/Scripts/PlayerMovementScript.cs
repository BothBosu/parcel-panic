using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 6.0f;
    public float jumpForce = 1.2f;
    public float rotationSpeed = 10.0f;
    public float gravity = -9.81f;

    [Header("Animation")]
    public Animator animator;

    // Components
    private CharacterController controller;
    private Transform cameraTransform;

    // Movement variables
    private Vector3 moveDirection;
    private float verticalVelocity;
    private bool isGrounded;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        // Check if grounded
        isGrounded = controller.isGrounded;

        // Reset vertical velocity when grounded
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -0.5f;
        }

        // Get input values
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Calculate move direction relative to camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Project vectors onto horizontal plane
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Set move direction
        moveDirection = forward * vertical + right * horizontal;

        // Determine speed based on running state
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Apply movement
        if (moveDirection.magnitude > 0.1f)
        {
            // Normalize for consistent speed
            moveDirection.Normalize();

            // Apply rotation (smooth look direction)
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            // Apply movement
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        // Handle jumping
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);

            // Trigger jump animation if animator exists
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }

        // Apply gravity
        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        // Update animations if animator exists
        if (animator != null)
        {
            animator.SetFloat("Speed", moveDirection.magnitude * currentSpeed);
            animator.SetBool("IsRunning", isRunning && moveDirection.magnitude > 0);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
}