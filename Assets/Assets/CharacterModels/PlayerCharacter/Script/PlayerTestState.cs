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

        Vector3 movement = new Vector3();

        // Move character
        movement.x = stateMachine.InputReader.MovementValue.x;
        movement.y = 0;
        movement.z = stateMachine.InputReader.MovementValue.y;

        stateMachine.CharacterController.Move(movement * stateMachine.FreeLookMovementSpeed * deltaTime);

        // Don't rotate if the character is not moving
        if (stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            // If player is not moving, set animation to Idle
            stateMachine.Animator.SetFloat("FreeLookSpeed", 0, 0.1f, deltaTime);
            return;
        }

        // If the character is moving, set animation to Walking
        stateMachine.Animator.SetFloat("FreeLookSpeed", 1, 0.1f, deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(movement);
        stateMachine.transform.rotation = Quaternion.Slerp(
            stateMachine.transform.rotation,
            targetRotation,
            rotationSpeed * deltaTime);
    }

    public override void Exit()
    {
        Debug.Log("Exiting walking state");
    }
}