using UnityEngine;

public class PlayerThrowingState : PlayerBaseState
{
    // Reference to the parcel and previous state
    private Transform carriedParcel;
    private PlayerBaseState previousState;

    // Trajectory visualization
    private LineRenderer lineRenderer;
    private GameObject targetIndicator;

    // Throw settings
    private float throwForce = 10.0f;
    private int trajectoryPoints = 30;
    private float trajectoryTimeStep = 0.05f;

    public PlayerThrowingState(PlayerStateMachine stateMachine, Transform parcel, PlayerBaseState previousState) : base(stateMachine)
    {
        this.carriedParcel = parcel;
        this.previousState = previousState;
    }

    public override void Enter()
    {
        Debug.Log("Entering throwing state");

        // Create trajectory visualization
        CreateTrajectoryObjects();

        // Initial update of trajectory
        UpdateTrajectory();
    }

    public override void Tick(float deltaTime)
    {
        // Check for input to exit the state
        if (Input.GetMouseButtonUp(1))
        {
            ExecuteThrow();
            return;
        }

        if (stateMachine.InputReader.JustPressedPickup)
        {
            CancelThrow();
            return;
        }

        // Update player rotation based on mouse position
        UpdatePlayerRotation(deltaTime);

        // Update trajectory based on current aim
        UpdateTrajectory();

        // Position the carried object
        PositionCarriedObject();
    }

    public override void Exit()
    {
        Debug.Log("Exiting throwing state");

        // Clean up trajectory objects
        if (lineRenderer != null)
        {
            Object.Destroy(lineRenderer.gameObject);
            lineRenderer = null;
        }

        if (targetIndicator != null)
        {
            Object.Destroy(targetIndicator);
            targetIndicator = null;
        }
    }

    private void CreateTrajectoryObjects()
    {
        // Create line renderer GameObject
        GameObject trajectoryObj = new GameObject("ThrowTrajectory");
        lineRenderer = trajectoryObj.AddComponent<LineRenderer>();

        // Configure line renderer
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = trajectoryPoints;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        // Create target indicator
        targetIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetIndicator.name = "TargetIndicator";
        targetIndicator.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        Renderer renderer = targetIndicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }

        // Disable collision on the indicator
        Collider collider = targetIndicator.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        Debug.Log("Trajectory objects created");
    }

    private void UpdatePlayerRotation(float deltaTime)
    {
        // Check for main camera
        if (Camera.main == null)
        {
            Debug.LogWarning("Main camera not found, cannot update rotation");
            return;
        }

        try
        {
            // Get the mouse position in screen space
            Vector3 mouseScreenPosition = Input.mousePosition;

            // Since we have a top-down camera, convert screen point to world point using a fixed Y value
            // This is a more direct way to get the world position under the cursor
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreenPosition.x, mouseScreenPosition.y,
                Camera.main.transform.position.y - stateMachine.transform.position.y));

            // Calculate direction to the mouse position on the ground
            Vector3 direction = worldPosition - stateMachine.transform.position;

            // Remove vertical component to keep rotation on the horizontal plane
            direction.y = 0;

            // Log for debugging
            Debug.Log($"Mouse world position: {worldPosition}, Direction: {direction}");

            // Only rotate if we have a valid direction
            if (direction.magnitude > 0.01f)
            {
                // Use direct rotation - no slerp for immediate response
                stateMachine.transform.rotation = Quaternion.LookRotation(direction);

                // Visual debugging
                Debug.DrawLine(stateMachine.transform.position,
                               stateMachine.transform.position + direction.normalized * 3f,
                               Color.red, 0.01f);

                Debug.DrawLine(stateMachine.transform.position, worldPosition, Color.blue, 0.01f);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UpdatePlayerRotation: {e.Message}\n{e.StackTrace}");
        }
    }

    private void UpdateTrajectory()
    {
        if (lineRenderer == null || targetIndicator == null)
        {
            Debug.LogError("Trajectory objects are missing!");
            return;
        }

        // Get starting position (front of player)
        Vector3 startPos = stateMachine.transform.position +
                          Vector3.up * 1.5f +
                          stateMachine.transform.forward * 0.5f;

        // Initial velocity in the forward direction of player with upward component
        Vector3 initialVelocity = stateMachine.transform.forward * throwForce + Vector3.up * 4f;

        // Calculate trajectory points
        Vector3[] points = new Vector3[trajectoryPoints];
        for (int i = 0; i < trajectoryPoints; i++)
        {
            // Time at this point
            float time = i * trajectoryTimeStep;

            // Calculate position using physics formula: p = p0 + v0t + 0.5at²
            points[i] = startPos +
                      initialVelocity * time +
                      0.5f * Physics.gravity * time * time;

            // Check for collision with environment
            if (i > 0)
            {
                RaycastHit hit;
                if (Physics.Linecast(points[i - 1], points[i], out hit))
                {
                    // End trajectory at hit point
                    points[i] = hit.point;

                    // Set all remaining points to hit point
                    for (int j = i + 1; j < trajectoryPoints; j++)
                    {
                        points[j] = hit.point;
                    }
                    break;
                }
            }
        }

        // Update line renderer
        lineRenderer.SetPositions(points);

        // Update target indicator position (end of trajectory)
        targetIndicator.transform.position = points[trajectoryPoints - 1];

        Debug.Log("Trajectory updated");
    }

    private void PositionCarriedObject()
    {
        if (carriedParcel == null) return;

        ParcelLogic parcel = carriedParcel.GetComponent<ParcelLogic>();
        if (parcel != null)
        {
            // Use parcel's positioning method
            parcel.PositionWhileCarrying(stateMachine.transform);
        }
    }

    private void ExecuteThrow()
    {
        Debug.Log("Throwing parcel");

        if (carriedParcel != null)
        {
            ParcelLogic parcel = carriedParcel.GetComponent<ParcelLogic>();
            if (parcel != null)
            {
                // Tell parcel it's being dropped
                parcel.Drop(stateMachine.transform.forward);

                // Apply throw force to rigidbody
                Rigidbody rb = carriedParcel.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Apply forward force with upward component
                    rb.linearVelocity = Vector3.zero;
                    rb.AddForce((stateMachine.transform.forward * throwForce + Vector3.up * 4f), ForceMode.Impulse);
                }
            }
        }

        // Return to previous state
        stateMachine.SwitchState(previousState);
    }

    private void CancelThrow()
    {
        Debug.Log("Canceling throw");
        stateMachine.SwitchState(previousState);
    }
}