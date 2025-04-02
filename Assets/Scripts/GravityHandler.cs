using UnityEngine;

public class GravityHandler : MonoBehaviour
{
    [Header("Gravity Settings")]
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float terminalVelocity = 53.0f;

    [Header("Ground Detection")]
    [SerializeField] private float groundedOffset = -0.14f;
    [SerializeField] private float groundedRadius = 0.28f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private bool showDebugGizmos = true;

    // Components
    private CharacterController characterController;

    // State
    private float verticalVelocity;
    private bool isGrounded;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        // Ensure we found the character controller
        if (characterController == null)
        {
            Debug.LogError("GravityHandler requires a CharacterController component!");
            enabled = false;
        }
    }

    private void Update()
    {
        CheckGrounded();
        ApplyGravity(Time.deltaTime);
    }

    private void CheckGrounded()
    {
        // Set sphere position, with offset
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y - groundedOffset,
            transform.position.z
        );

        // Check if player is grounded
        isGrounded = Physics.CheckSphere(
            spherePosition,
            groundedRadius,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    private void ApplyGravity(float deltaTime)
    {
        // If grounded and velocity is negative, reset it to a small negative value
        if (isGrounded && verticalVelocity < 0.0f)
        {
            verticalVelocity = -2f; // Small negative value helps stick to ground
        }
        else
        {
            // Add gravity over time up to terminal velocity
            verticalVelocity -= gravity * deltaTime;

            // Clamp at terminal velocity
            if (verticalVelocity < -terminalVelocity)
            {
                verticalVelocity = -terminalVelocity;
            }
        }
    }

    // Visualization for ground check
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = isGrounded ? transparentGreen : transparentRed;

        // Draw sphere at feet for ground check
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y - groundedOffset,
            transform.position.z
        );

        Gizmos.DrawSphere(spherePosition, groundedRadius);
    }
}