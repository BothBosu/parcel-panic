using UnityEngine;

public class PlayerRunningState : PlayerBaseState
{
    // How much faster the character runs compared to walking
    private readonly float runSpeedMultiplier = 2.0f;

    // How quickly the character rotates to face the movement direction (higher = faster)
    private readonly float rotationSpeed = 10.0f;

    public PlayerRunningState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("Entering running state");
        stateMachine.Animator.SetFloat("FreeLookSpeed", 2.0f, 0.1f, Time.deltaTime);
    }

    public override void Tick(float deltaTime)
    {
        // Check for state transition first - if no longer running, switch back to normal movement
        if (!stateMachine.InputReader.IsRunning)
        {
            stateMachine.SwitchState(new PlayerTestState(stateMachine));
            return;
        }

        // Handle movement and rotation
        Vector3 movement = new Vector3();

        // Convert input to movement vector
        movement.x = stateMachine.InputReader.MovementValue.x;
        movement.y = 0;
        movement.z = stateMachine.InputReader.MovementValue.y;

        // Apply movement with running speed multiplier
        stateMachine.CharacterController.Move(
            movement * stateMachine.FreeLookMovementSpeed * runSpeedMultiplier * deltaTime);

        // Don't rotate if the character is not moving
        if (stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            // If player stops moving while running, transition back to normal state
            stateMachine.SwitchState(new PlayerTestState(stateMachine));
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(movement);
        stateMachine.transform.rotation = Quaternion.Slerp(
            stateMachine.transform.rotation,
            targetRotation,
            rotationSpeed * deltaTime);

        // Keep animation parameter updated
        stateMachine.Animator.SetFloat("FreeLookSpeed", 2.0f, 0.1f, deltaTime);
    }

    public override void Exit()
    {
        Debug.Log("Exiting running state");
    }
}