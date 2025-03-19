using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public float FreeLookMovementSpeed { get; private set; }
    [field: SerializeField] public bool IsCarrying { get; private set; } = false;

    [SerializeField] private string carryingParameterName = "IsCarrying";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SwitchState(new PlayerTestState(this));
    }

    // Method to be called by the Parcel script when picked up or dropped
    public void SetCarryingState(bool carrying)
    {
        IsCarrying = carrying;
        Animator.SetBool("IsCarrying", carrying);
    }
}