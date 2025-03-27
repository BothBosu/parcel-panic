using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public float FreeLookMovementSpeed { get; private set; }
    [field: SerializeField] public bool IsCarrying { get; private set; } = false;
    [field: SerializeField] public bool IsGrounded { get; private set; } = true;

    [SerializeField] private string carryingParameterName = "IsCarrying";
    [SerializeField] private string jumpTriggerName = "Jump";
    [SerializeField] private string isGroundedName = "IsGrounded";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SwitchState(new PlayerTestState(this));
    }

    // Method to be called by the Parcel script when picked up or dropped
    public void SetCarryingState(bool carrying)
    {
        IsCarrying = carrying;
        Animator.SetBool(carryingParameterName, carrying);
    }

    // New method to trigger jump animation
    public void TriggerJump()
    {
        Animator.SetTrigger(jumpTriggerName);
    }

    // New method to set the grounded state
    public void SetGroundedState(bool grounded)
    {
        IsGrounded = grounded;
        Animator.SetBool(isGroundedName, grounded);
    }
}