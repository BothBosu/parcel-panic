using System.Collections;
using UnityEngine;

public class PlayerImpactState : PlayerBaseState
{
    private Vector3 impactDirection;
    private float impactForce;
    private float recoveryTime;
    private float elapsedTime;
    private float verticalVelocity;
    private float gravityValue = -9.81f;
    
    public PlayerImpactState(
        PlayerStateMachine stateMachine, 
        Vector3 direction,
        float force,
        float recovery = 0.5f) : base(stateMachine)
    {
        this.impactDirection = direction;
        this.impactForce = force;
        this.recoveryTime = recovery;
    }

    public override void Enter()
    {
        Debug.Log("Entering impact state");
        
        // Reset elapsed time
        elapsedTime = 0f;
        
        // Trigger impact animation if available
        stateMachine.Animator.SetTrigger("Impact");
        
        // Calculate vertical factor - less upward force for side impacts
        float horizontalMagnitude = new Vector2(impactDirection.x, impactDirection.z).magnitude;
        float verticalFactor = Mathf.Clamp01(1.0f - horizontalMagnitude);
        
        // Add a small upward velocity for all impacts, but much less for side impacts
        verticalVelocity = verticalFactor * 3f; // Small initial upward velocity
        
        // Start the coroutine to handle impact recovery
        stateMachine.StartCoroutine(RecoverFromImpact());
    }

    public override void Tick(float deltaTime)
    {
        // This state is mostly handled by the coroutine
        // But we can add any additional per-frame logic here if needed
    }

    private IEnumerator RecoverFromImpact()
    {
        while (elapsedTime < recoveryTime)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;
            
            // Calculate impact force with decay over time
            float forceFactor = 1.0f - (elapsedTime / recoveryTime);
            
            // Keep impact primarily horizontal for side impacts
            Vector3 currentImpact = impactDirection * impactForce * forceFactor;
            
            // Apply gravity to vertical velocity
            verticalVelocity += gravityValue * deltaTime;
            
            // Add vertical component to the impact
            currentImpact.y = verticalVelocity * deltaTime;
            
            // Apply movement if character controller is active
            if (stateMachine.CharacterController != null && 
                stateMachine.CharacterController.enabled && 
                stateMachine.CharacterController.gameObject.activeInHierarchy)
            {
                stateMachine.CharacterController.Move(currentImpact * deltaTime);
            }
            
            yield return null;
        }
        
        // Transition back to the appropriate state
        stateMachine.SwitchState(new PlayerTestState(stateMachine));
    }

    public override void Exit()
    {
        Debug.Log("Exiting impact state");
    }
}