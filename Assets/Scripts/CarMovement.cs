using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public enum MovementType
    {
        BackAndForth,
        Patrol,
        Custom
    }

    public MovementType movementType = MovementType.BackAndForth;
    public float speed = 5f;
    public float rotationSpeed = 50f;
    public Transform[] waypoints; // For patrol movement
    
    private int currentWaypointIndex = 0;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool movingForward = true;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        
        if (movementType == MovementType.BackAndForth && waypoints.Length >= 2)
        {
            startPosition = waypoints[0].position;
            endPosition = waypoints[1].position;
            transform.position = startPosition;
        }
    }

    void FixedUpdate()
    {
        switch (movementType)
        {
            case MovementType.BackAndForth:
                MoveBackAndForth();
                break;
            case MovementType.Patrol:
                MovePatrol();
                break;
            case MovementType.Custom:
                // Add your custom movement logic here
                break;
        }
    }

    void MoveBackAndForth()
    {
        Vector3 targetPosition = movingForward ? endPosition : startPosition;
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        
        // Rotate towards movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Move the car
        rb.MovePosition(transform.position + moveDirection * speed * Time.deltaTime);
        
        // Check if we need to change direction
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            movingForward = !movingForward;
        }
    }

    void MovePatrol()
    {
        if (waypoints.Length == 0)
            return;
            
        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        
        // Rotate towards movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Move the car
        rb.MovePosition(transform.position + moveDirection * speed * Time.deltaTime);
        
        // Check if we reached the waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}