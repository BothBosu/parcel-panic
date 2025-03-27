using UnityEngine;

public class PlayerTestState : PlayerBaseState
{
    // How quickly the character rotates to face the movement direction
    private readonly float rotationSpeed = 10.0f;

    public PlayerTestState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("Entering walking state");

        // Make sure the carrying animation parameter is properly set
        stateMachine.Animator.SetBool("IsCarrying", stateMachine.IsCarrying);
    }

    public override void Tick(float deltaTime)
{
    // Check if player wants to run and is also moving
    if (stateMachine.InputReader.IsRunning && stateMachine.InputReader.MovementValue != Vector2.zero)
    {
        stateMachine.SwitchState(new PlayerRunningState(stateMachine));
        return;
    }

    // Get the camera's forward and right vectors, but zero out their Y components
    // Assuming you have a reference to the main camera or a camera transform
    Transform cameraTransform = Camera.main.transform;
    Vector3 cameraForward = cameraTransform.forward;
    Vector3 cameraRight = cameraTransform.right;
    
    cameraForward.y = 0;
    cameraRight.y = 0;
    cameraForward.Normalize();
    cameraRight.Normalize();

    // Calculate movement direction relative to camera orientation
    Vector3 movement = cameraRight * stateMachine.InputReader.MovementValue.x + 
                      cameraForward * stateMachine.InputReader.MovementValue.y;
    
    // Apply movement speed
    Vector3 movementVelocity = movement * stateMachine.FreeLookMovementSpeed * deltaTime;

    // Move the character
    if (stateMachine.CharacterController != null && 
        stateMachine.CharacterController.enabled && 
        stateMachine.CharacterController.gameObject.activeInHierarchy)
    {
        stateMachine.CharacterController.Move(movementVelocity);
    }

    // Handle rotation - don't rotate if not moving
    if (stateMachine.InputReader.MovementValue == Vector2.zero)
    {
        // If player is not moving, set animation to Idle
        stateMachine.Animator.SetFloat("FreeLookSpeed", 0, 0.1f, deltaTime);
        return;
    }

    // If the character is moving, set animation to Walking
    stateMachine.Animator.SetFloat("FreeLookSpeed", 1, 0.1f, deltaTime);

    // Rotate the character to face the movement direction
    if (movement != Vector3.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(movement);
        stateMachine.transform.rotation = Quaternion.Slerp(
            stateMachine.transform.rotation,
            targetRotation,
            rotationSpeed * deltaTime);
    }
}

    public override void Exit()
    {
        Debug.Log("Exiting walking state");
    }
}