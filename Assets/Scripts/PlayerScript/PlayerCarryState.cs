using UnityEngine;

public class PlayerCarryState : PlayerBaseState
{
    // Reference to the parcel being carried
    private Transform carriedParcel;

    // How quickly the character rotates to face movement direction
    private readonly float rotationSpeed = 10.0f;

    // Position offset for the carried object
    private readonly Vector3 carryOffset = new Vector3(0f, 0.5f, 0f);

    // The previous state (to return to when dropping)
    private PlayerBaseState previousState;

    public PlayerCarryState(PlayerStateMachine stateMachine, Transform parcel, PlayerBaseState previousState) : base(stateMachine)
    {
        this.carriedParcel = parcel;
        this.previousState = previousState;
    }

    public override void Enter()
    {
        Debug.Log("Entering carry state");

        // Set animation parameter for carrying
        stateMachine.Animator.SetBool("IsCarrying", true);
    }

    public override void Tick(float deltaTime)
    {
        // Handle input for running while carrying
        if (stateMachine.InputReader.IsRunning && stateMachine.InputReader.MovementValue != Vector2.zero)
        {
            SwitchToRunningCarryState();
            return;
        }

        // Handle movement similar to normal movement state
        Vector3 movement = new Vector3();
        movement.x = stateMachine.InputReader.MovementValue.x;
        movement.y = 0;
        movement.z = stateMachine.InputReader.MovementValue.y;

        // Apply movement
        stateMachine.CharacterController.Move(movement * stateMachine.FreeLookMovementSpeed * deltaTime);

        // Update animation speed parameter
        if (stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            // If player is not moving, set animation to Idle
            stateMachine.Animator.SetFloat("FreeLookSpeed", 0, 0.1f, deltaTime);
        }
        else
        {
            // If the character is moving, set animation to Walking
            stateMachine.Animator.SetFloat("FreeLookSpeed", 1, 0.1f, deltaTime);

            // Rotate to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            stateMachine.transform.rotation = Quaternion.Slerp(
                stateMachine.transform.rotation,
                targetRotation,
                rotationSpeed * deltaTime);
        }

        // Update carried object position
        PositionCarriedObject();
    }

    public override void Exit()
    {
        Debug.Log("Exiting carry state");

        // Reset carrying animation parameter
        stateMachine.Animator.SetBool("IsCarrying", false);
    }

    private void SwitchToRunningCarryState()
    {
        // Create a running carry state and switch to it
        PlayerRunningCarryState runningCarryState =
            new PlayerRunningCarryState(stateMachine, carriedParcel, previousState);
        stateMachine.SwitchState(runningCarryState);
    }

    private void PositionCarriedObject()
    {
        // Position the object relative to the player using the Parcel's positioning method
        if (carriedParcel != null)
        {
            ParcelLogic parcel = carriedParcel.GetComponent<ParcelLogic>();

            if (parcel != null)
            {
                // Use the Parcel's custom positioning method
                parcel.PositionWhileCarrying(stateMachine.transform);
            }
            else
            {
                // Fallback to default positioning if no Parcel component is found
                Vector3 carryPosition = stateMachine.transform.position +
                                       Vector3.up * stateMachine.transform.localScale.y +
                                       carryOffset;

                carriedParcel.position = carryPosition;
                carriedParcel.rotation = stateMachine.transform.rotation;
            }
        }
    }
}