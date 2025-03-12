using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;  // <--- add

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))] // <--- add
public class PlayerController : MonoBehaviour   // <--- Change the classname to PlayerController
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    private float playerSpeed = 2.0f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;

    private PlayerInput playerInput;  // <--- add

    private InputAction moveAction;  // <--- add
    private InputAction lookAction;  // <--- add
    private InputAction jumpAction;  // <--- add
    private InputAction aimAction;  // <--- add


    private void Start()
    { 
        controller = GetComponent<CharacterController>();  // <--- add
        playerInput = GetComponent<PlayerInput>();  // <--- add
        moveAction = playerInput.actions["Move"];  // <--- add
        lookAction = playerInput.actions["Look"];  // <--- add
        jumpAction = playerInput.actions["Jump"];  // <--- add
        aimAction = playerInput.actions["Aim"];  // <--- add
    }

    void Update()
    {
        // gravity
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0) // If grounded or the physics engine make it less than 0
        {
            playerVelocity.y = 0f;                  // we force it to 0
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                                                    // Notice that this vector is x,0,z ... this is the x-z plane
                                                    // Don't get confused with the word "Horizontal" and "Vertical".
                                                    // The two words are about the MOUSE or WASD keys.
                                                    // Horizontal moves result in x movement in the game.
                                                    // Vertical moves result in z movement in the game.
        controller.Move(move * Time.deltaTime * playerSpeed); // Here, make the move

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;   // Make the face turn to that direction
        }

        // Makes the player jump
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}

