using UnityEngine;
using System.Collections;

public class ParcelLogic : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float pickupDistance = 2.0f;      // Maximum distance to pickup the parcel
    [SerializeField] private float dropForce = 2.0f;           // Force applied when dropping
    [SerializeField] private float dropDistance = 1.5f;        // Distance in front of player to drop the parcel
    [SerializeField] private float collisionIgnoreTime = 1.0f; // How long to ignore player collisions after dropping
    [SerializeField] private LayerMask playerLayerMask;        // Layer mask for player detection

    [Header("Carrying Position Adjustment")]
    [SerializeField] private float offsetX = 0.0f;
    [SerializeField] private float offsetY = 0.0f;
    [SerializeField] private float offsetZ = 0.0f;
    [SerializeField] private string targetBoneName = "";

    // Expose pickup distance for ParcelManager
    public float PickupDistance => pickupDistance;

    // Expose pickup status
    public bool IsPickedUp { get; private set; } = false;

    // References
    private PlayerStateMachine playerStateMachine;             // Reference to the PlayerStateMachine
    private Rigidbody rb;                                      // Rigidbody of the parcel
    private Collider parcelCollider;                           // Collider of the parcel
    private Transform targetBone;                              // Reference to the target bone transform

    private void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        parcelCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        // Find and cache the PlayerStateMachine
        playerStateMachine = FindFirstObjectByType<PlayerStateMachine>();
        if (playerStateMachine != null)
        {
            // Try to find the target bone if specified
            if (!string.IsNullOrEmpty(targetBoneName) && playerStateMachine.Animator != null)
            {
                targetBone = FindBoneTransform(playerStateMachine.Animator.transform, targetBoneName);
                if (targetBone == null)
                {
                    Debug.LogWarning($"Target bone '{targetBoneName}' not found on character.");
                }
                else
                {
                    Debug.Log($"Found target bone: {targetBoneName}");
                }
            }
        }
        else
        {
            Debug.LogError("Cannot find PlayerStateMachine in the scene!");
        }

        // Register with ParcelManager
        ParcelManager.Instance.RegisterParcel(this);
    }

    private void OnDestroy()
    {
        // Unregister from ParcelManager
        ParcelManager.Instance.UnregisterParcel(this);
    }

    // Called by ParcelManager when this parcel is selected for pickup
    public void HandlePickup(PlayerBaseState previousState)
    {
        if (IsPickedUp) return;

        IsPickedUp = true;

        // Disable physics
        rb.isKinematic = true;
        parcelCollider.enabled = false;

        // Switch to the carry state
        playerStateMachine.SwitchState(new PlayerCarryState(playerStateMachine, transform, previousState));

        Debug.Log("Parcel picked up");
    }

    // Public method to get the pickup target position (used by ParcelManager)
    public Vector3 GetPickupTargetPosition()
    {
        // By default, use the center of the object
        if (parcelCollider is BoxCollider boxCollider)
        {
            // For box colliders, use the center of the box
            return transform.position + boxCollider.center;
        }
        else if (parcelCollider is SphereCollider sphereCollider)
        {
            // For sphere colliders, use the center of the sphere
            return transform.position + sphereCollider.center;
        }
        else
        {
            // For other collider types, just use the transform position
            return transform.position;
        }
    }

    public void Drop(Vector3 dropDirection)
    {
        if (!IsPickedUp) return;

        IsPickedUp = false;

        // Find a valid drop position that isn't inside a wall
        Vector3 dropPosition = FindValidDropPosition(dropDirection);

        // Position the parcel at the valid drop position
        transform.position = dropPosition;

        // Re-enable physics
        rb.isKinematic = false;
        parcelCollider.enabled = true;

        // Apply force to move the parcel forward away from the player
        rb.AddForce(dropDirection * dropForce, ForceMode.Impulse);

        // Temporarily ignore collisions between player and parcel
        StartCoroutine(TemporarilyIgnorePlayerCollision());

        Debug.Log("Parcel dropped");
    }

    // Positions the parcel relative to the character
    public void PositionWhileCarrying(Transform characterTransform)
    {
        if (!IsPickedUp) return;

        if (targetBone != null)
        {
            // Position relative to the target bone with offset
            Vector3 bonePosition = targetBone.position;
            Quaternion boneRotation = targetBone.rotation;

            // Apply local offsets in bone-space
            Vector3 localOffset = new Vector3(offsetX, offsetY, offsetZ);
            Vector3 worldOffset = boneRotation * localOffset;

            // Set position and rotation
            transform.position = bonePosition + worldOffset;
            transform.rotation = boneRotation;
        }
        else
        {
            // Fallback to positioning relative to the character if no bone is found
            Vector3 basePosition = characterTransform.position;

            // Create a position based on the character's orientation
            Vector3 right = characterTransform.right * offsetX;
            Vector3 up = Vector3.up * (characterTransform.localScale.y + offsetY);
            Vector3 forward = characterTransform.forward * offsetZ;

            // Combine all offsets
            Vector3 finalPosition = basePosition + right + up + forward;

            // Update position and rotation
            transform.position = finalPosition;
            transform.rotation = characterTransform.rotation;
        }
    }

    private System.Collections.IEnumerator TemporarilyIgnorePlayerCollision()
    {
        // Get the player's collider
        Collider playerCollider = playerStateMachine.GetComponent<Collider>();
        if (playerCollider != null)
        {
            // Ignore collision between player and parcel
            Physics.IgnoreCollision(parcelCollider, playerCollider, true);

            // Wait for the specified ignore time
            yield return new WaitForSeconds(collisionIgnoreTime);

            // Re-enable collision
            Physics.IgnoreCollision(parcelCollider, playerCollider, false);
        }
    }

    private Vector3 FindValidDropPosition(Vector3 dropDirection)
    {
        Transform playerTransform = playerStateMachine.transform;
        Vector3 playerPosition = playerTransform.position;

        // Start with the ideal drop position (in front of player)
        Vector3 idealDropPosition = playerPosition + dropDirection * dropDistance;

        // Debug visualization
        Debug.DrawLine(playerPosition, idealDropPosition, Color.blue, 3.0f);

        // Get parcel dimensions
        Vector3 parcelExtents = GetParcelExtents();
        float checkRadius = Mathf.Max(parcelExtents.x, parcelExtents.z) + 0.05f;

        // Temporarily disable the parcel's collider so it doesn't interfere with raycasts
        bool originalColliderState = parcelCollider.enabled;
        parcelCollider.enabled = false;

        // 1. First, check if there's a direct wall between player and drop position
        bool wallDetected = false;
        float safestDistance = dropDistance;

        // Create an array of test points (center, top, bottom, left, right) relative to ideal position
        Vector3[] testOffsets = new Vector3[]
        {
            Vector3.zero,                          // Center
            new Vector3(0, parcelExtents.y, 0),    // Top
            new Vector3(0, -parcelExtents.y, 0),   // Bottom
            new Vector3(parcelExtents.x, 0, 0),    // Right
            new Vector3(-parcelExtents.x, 0, 0),   // Left
            new Vector3(0, 0, parcelExtents.z),    // Front
            new Vector3(0, 0, -parcelExtents.z)    // Back
        };

        // Calculate drop position that rotates with the player's direction
        Quaternion playerRotation = Quaternion.LookRotation(dropDirection);

        // Check all test points with raycasts
        foreach (Vector3 offset in testOffsets)
        {
            // Rotate the offset based on player direction
            Vector3 rotatedOffset = playerRotation * offset;

            // Calculate start and end points
            Vector3 testStart = playerPosition + Vector3.up * 0.8f; // Start from player's approximate chest height
            Vector3 testEnd = idealDropPosition + rotatedOffset;
            Vector3 testDirection = (testEnd - testStart).normalized;
            float testDistance = Vector3.Distance(testStart, testEnd);

            // Debug visualization
            Debug.DrawRay(testStart, testDirection * testDistance, Color.yellow, 3.0f);

            // Check for obstacles
            RaycastHit hit;
            if (Physics.Raycast(testStart, testDirection, out hit, testDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                // Skip if we hit the player
                if (hit.collider.gameObject == playerStateMachine.gameObject)
                    continue;

                // Skip if we hit a trigger
                if (hit.collider.isTrigger)
                    continue;

                // We hit something - calculate how far we can safely go
                wallDetected = true;
                float hitDistance = hit.distance * 0.8f; // 80% of the distance for safety

                // Keep track of the closest obstacle
                if (hitDistance < safestDistance)
                {
                    safestDistance = hitDistance;
                    Debug.DrawRay(hit.point, hit.normal, Color.red, 3.0f);
                    Debug.Log($"Wall detected: {hit.collider.gameObject.name} at distance {hitDistance}");
                }
            }
        }

        // Determine final drop position
        Vector3 finalDropPosition;

        if (wallDetected)
        {
            // If wall detected, use the safest distance (at least 0.5m from player)
            safestDistance = Mathf.Max(safestDistance, 0.5f);
            finalDropPosition = playerPosition + dropDirection * safestDistance;
            Debug.DrawLine(playerPosition, finalDropPosition, Color.red, 3.0f);
        }
        else
        {
            // No walls detected, use ideal position
            finalDropPosition = idealDropPosition;
        }

        // 2. Now check for ground below drop position
        RaycastHit groundHit;
        Vector3 groundCheckStart = finalDropPosition + Vector3.up * 2.0f;
        if (Physics.Raycast(groundCheckStart, Vector3.down, out groundHit, 4.0f))
        {
            // Position above ground
            finalDropPosition.y = groundHit.point.y + parcelExtents.y + 0.1f;
            Debug.DrawLine(groundCheckStart, groundHit.point, Color.green, 3.0f);
        }
        else
        {
            // No ground found, use player's height
            finalDropPosition.y = playerPosition.y;
        }

        // 3. Final validation - check if the position would put the parcel inside anything
        if (Physics.CheckSphere(finalDropPosition, checkRadius, ~0, QueryTriggerInteraction.Ignore))
        {
            // Position would cause intersection, fall back to a safer position
            Debug.LogWarning("Final position would cause intersection - falling back to safety position");
            finalDropPosition = playerPosition + dropDirection * 0.5f + Vector3.up * parcelExtents.y;
        }

        // Re-enable parcel collider
        parcelCollider.enabled = originalColliderState;

        return finalDropPosition;
    }

    // Helper method to determine parcel size based on collider type
    private Vector3 GetParcelExtents()
    {
        if (parcelCollider is BoxCollider boxCollider)
        {
            // Use box size, accounting for scale
            return Vector3.Scale(boxCollider.size * 0.5f, transform.lossyScale);
        }
        else if (parcelCollider is SphereCollider sphereCollider)
        {
            // For sphere, use radius for all dimensions, accounting for scale
            float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            return Vector3.one * sphereCollider.radius * maxScale;
        }
        else if (parcelCollider is CapsuleCollider capsuleCollider)
        {
            // For capsule, handle different orientations
            Vector3 extents = Vector3.one * capsuleCollider.radius;

            // Adjust height based on capsule direction
            float height = capsuleCollider.height * 0.5f;
            if (capsuleCollider.direction == 0) // X-axis
                extents.x = height;
            else if (capsuleCollider.direction == 1) // Y-axis
                extents.y = height;
            else // Z-axis
                extents.z = height;

            // Account for object scale
            return Vector3.Scale(extents, transform.lossyScale);
        }
        else
        {
            // For other collider types, use bounds
            return parcelCollider.bounds.extents;
        }
    }

    // Recursively searches through the bone hierarchy to find a bone by name
    private Transform FindBoneTransform(Transform current, string boneName)
    {
        // Check if this is the bone we're looking for
        if (current.name.Contains(boneName))
        {
            return current;
        }

        // Search through all children
        foreach (Transform child in current)
        {
            Transform result = FindBoneTransform(child, boneName);
            if (result != null)
            {
                return result;
            }
        }

        // Not found in this branch
        return null;
    }
}