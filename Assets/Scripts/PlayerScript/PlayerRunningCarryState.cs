using UnityEngine;

public class PlayerRunningCarryState : PlayerBaseState
{
    // Reference to the parcel being carried
    private Transform carriedParcel;

    // How much faster the character runs compared to walking
    private readonly float runSpeedMultiplier = 1.10f;

    // How quickly the character rotates to face movement direction
    private readonly float rotationSpeed = 10.0f;

    // Position offset for the carried object
    private readonly Vector3 carryOffset = new Vector3(0f, 0.5f, 0f);

    // The previous state (to return to when dropping)
    private PlayerBaseState previousState;

    public PlayerRunningCarryState(PlayerStateMachine stateMachine, Transform parcel, PlayerBaseState previousState) : base(stateMachine)
    {
        this.carriedParcel = parcel;
        this.previousState = previousState;
    }

    public override void Enter()
    {
        Debug.Log("Entering running carry state");

        // Set animation parameters for running and carrying
        stateMachine.Animator.SetBool("IsCarrying", true);
        stateMachine.Animator.SetFloat("FreeLookSpeed", 2.0f, 0.1f, Time.deltaTime);
    }

    public override void Tick(float deltaTime)
    {
        // Check for state transition - if no longer running, switch back to normal carry state
        if (!stateMachine.InputReader.IsRunning)
        {
            stateMachine.SwitchState(new PlayerCarryState(stateMachine, carriedParcel, previousState));
            return;
        }

        // Handle movement with running speed
        Vector3 movement = new Vector3();
        movement.x = stateMachine.InputReader.MovementValue.x;
        movement.y = 0;
        movement.z = stateMachine.InputReader.MovementValue.y;

        // Apply movement with running speed multiplier
        stateMachine.CharacterController.Move(
            movement * stateMachine.FreeLookMovementSpeed * runSpeedMultiplier * deltaTime);

        // If player stops moving while running, transition back to normal carrying state
        if (stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            stateMachine.SwitchState(new PlayerCarryState(stateMachine, carriedParcel, previousState));
            return;
        }

        // Rotate to face movement direction
        Quaternion targetRotation = Quaternion.LookRotation(movement);
        stateMachine.transform.rotation = Quaternion.Slerp(
            stateMachine.transform.rotation,
            targetRotation,
            rotationSpeed * deltaTime);

        // Update animation parameter
        stateMachine.Animator.SetFloat("FreeLookSpeed", 2.0f, 0.1f, deltaTime);

        // Update carried object position
        PositionCarriedObject();
    }

    public override void Exit()
    {
        Debug.Log("Exiting running carry state");

        // Note: We don't reset IsCarrying here as we might be transitioning to another carry state
        // It will be reset when exiting the carry state completely
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