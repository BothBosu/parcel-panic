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
}