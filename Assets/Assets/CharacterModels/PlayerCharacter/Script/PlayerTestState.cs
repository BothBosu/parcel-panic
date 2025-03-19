using System.Threading;
using UnityEngine;

public class PlayerTestState : PlayerBaseState
{
    public PlayerTestState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        
    }

    public override void Enter()
    {
        
    }

    public override void Tick(float deltaTime)
    {
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

        // If the character is moving, set animation to Running
        stateMachine.Animator.SetFloat("FreeLookSpeed", 1, 0.1f, deltaTime);
        stateMachine.transform.rotation = Quaternion.LookRotation(movement);
    }

    public override void Exit()
    {
        Debug.Log("Exit");
    }
}
