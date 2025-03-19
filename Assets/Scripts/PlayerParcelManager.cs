using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParcelManager : MonoBehaviour
{
    [Header("Parcel Collection")]
    public int maxParcels = 1; // How many parcels the player can hold
    public Transform holdPoint; // Where the parcel appears when held
    
    [Header("Throwing Settings")]
    public float minThrowForce = 5f;
    public float maxThrowForce = 30f;
    public float chargeTime = 2f; // Time to reach max throw force
    public float throwUpwardAngle = 20f; // Upward angle for the throw
    
    [Header("Trajectory Visualization")]
    public TrajectoryRenderer trajectoryRenderer;
    public int trajectorySteps = 30; // Number of points in the trajectory line
    public float trajectoryTimeStep = 0.1f; // Time between each point in the trajectory
    
    [Header("Sound Effects")]
    public AudioClip throwSound; // Sound played when releasing throw
    public AudioClip chargingSound; // Sound played when charging throw
    public AudioClip collectSound; // Sound played when collecting a parcel
    
    // Private variables
    private List<ParcelController> heldParcels = new List<ParcelController>();
    private float currentChargeTime = 0f;
    private bool isCharging = false;
    private Camera mainCamera;
    
    // Audio sources
    private AudioSource throwAudioSource;
    private AudioSource chargingAudioSource;
    private AudioSource collectAudioSource;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        // Make sure trajectory renderer is assigned
        if (trajectoryRenderer == null)
        {
            Debug.LogError("Trajectory Renderer is not assigned to Player Parcel Manager!");
        }
        
        // Setup audio sources
        SetupAudioSources();
    }
    
    private void SetupAudioSources()
    {
        // Create audio source for throw sound
        throwAudioSource = gameObject.AddComponent<AudioSource>();
        throwAudioSource.clip = throwSound;
        throwAudioSource.playOnAwake = false;
        throwAudioSource.volume = 1.0f;
        
        // Create audio source for charging sound
        chargingAudioSource = gameObject.AddComponent<AudioSource>();
        chargingAudioSource.clip = chargingSound;
        chargingAudioSource.playOnAwake = false;
        chargingAudioSource.loop = true; // Loop the charging sound
        chargingAudioSource.volume = 0.7f;
        
        // Create audio source for collect sound
        collectAudioSource = gameObject.AddComponent<AudioSource>();
        collectAudioSource.clip = collectSound;
        collectAudioSource.playOnAwake = false;
        collectAudioSource.volume = 0.8f;
    }
    
    private void Update()
    {
        // Only proceed if we have a parcel to throw
        if (heldParcels.Count > 0)
        {
            // Start charging throw on mouse down
            if (Input.GetMouseButtonDown(0))
            {
                StartCharging();
            }
            
            // Continue charging while mouse is held
            if (Input.GetMouseButton(0) && isCharging)
            {
                ContinueCharging();
            }
            
            // Throw on mouse up
            if (Input.GetMouseButtonUp(0) && isCharging)
            {
                ThrowParcel();
            }
        }
        else
        {
            // Hide trajectory if no parcels are held
            if (trajectoryRenderer != null)
            {
                trajectoryRenderer.HideTrajectory();
            }
        }
    }
    
    public bool CollectParcel(ParcelController parcel)
    {
        // Check if we can collect more parcels
        if (heldParcels.Count >= maxParcels)
        {
            return false;
        }
        
        // Add the parcel to our held parcels
        heldParcels.Add(parcel);
        
        // Position the parcel at the hold point and parent it to the player
        parcel.transform.position = holdPoint.position;
        parcel.transform.parent = holdPoint;
        
        // Disable the parcel's rigidbody while held
        Rigidbody rb = parcel.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Play collect sound
        if (collectAudioSource != null && collectSound != null)
        {
            collectAudioSource.Play();
        }

        return true;
    }
    
    private void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0f;
        
        // Begin showing trajectory
        UpdateTrajectory(GetCurrentThrowForce());
        
        // Play charging sound
        if (chargingAudioSource != null && chargingSound != null)
        {
            chargingAudioSource.Play();
        }
    }
    
    private void ContinueCharging()
    {
        // Increase charge time up to the max
        currentChargeTime = Mathf.Min(currentChargeTime + Time.deltaTime, chargeTime);
        
        // Update trajectory visualization
        UpdateTrajectory(GetCurrentThrowForce());
    }
    
    private void ThrowParcel()
    {
        if (heldParcels.Count > 0)
        {
            // Get the first parcel in our list
            ParcelController parcelToThrow = heldParcels[0];
            heldParcels.RemoveAt(0);

            // Record the time when this parcel was thrown
            parcelToThrow.lastThrownTime = Time.time;
            
            // Reactivate the parcel
            parcelToThrow.gameObject.SetActive(true);
            
            // Position the parcel at our throw point
            parcelToThrow.transform.position = holdPoint.position;
            
            // Unparent from the hold point
            parcelToThrow.transform.parent = null;
            
            // Make sure it has a rigidbody for physics
            Rigidbody rb = parcelToThrow.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = parcelToThrow.gameObject.AddComponent<Rigidbody>();
            }
            
            // This is the important part - set isKinematic to false before applying velocity
            rb.isKinematic = false;
            
            // Calculate throw direction
            Vector3 throwDirection = GetThrowDirection();
            
            // Apply throw force
            float throwForce = GetCurrentThrowForce();
            rb.linearVelocity = throwDirection * throwForce;
            
            // Stop charging sound
            if (chargingAudioSource != null && chargingAudioSource.isPlaying)
            {
                chargingAudioSource.Stop();
            }
            
            // Play throw sound
            if (throwAudioSource != null && throwSound != null)
            {
                throwAudioSource.Play();
            }
            
            // Reset charging state
            isCharging = false;
            currentChargeTime = 0f;
            
            // Hide trajectory
            if (trajectoryRenderer != null)
            {
                trajectoryRenderer.HideTrajectory();
            }
        }
    }
    
    private float GetCurrentThrowForce()
    {
        // Calculate force based on charge time
        float chargePercentage = currentChargeTime / chargeTime;
        return Mathf.Lerp(minThrowForce, maxThrowForce, chargePercentage);
    }
    
    private Vector3 GetThrowDirection()
    {
        // By default, throw in the direction the player is facing
        Vector3 direction = transform.forward;
        
        // You can also throw towards where the camera is pointing
        if (mainCamera != null)
        {
            direction = mainCamera.transform.forward;
        }
        
        // Apply upward angle
        Quaternion upwardRotation = Quaternion.Euler(-throwUpwardAngle, 0, 0);
        direction = upwardRotation * direction;
        
        return direction.normalized;
    }
    
    private void UpdateTrajectory(float throwForce)
    {
        if (trajectoryRenderer == null) return;
        
        Vector3 startPosition = holdPoint.position;
        Vector3 startVelocity = GetThrowDirection() * throwForce;
        
        // Calculate points along the trajectory
        Vector3[] trajectoryPoints = CalculateTrajectoryPoints(startPosition, startVelocity, trajectorySteps, trajectoryTimeStep);
        
        // Update the trajectory visualization
        trajectoryRenderer.ShowTrajectory(trajectoryPoints);
    }
    
    private Vector3[] CalculateTrajectoryPoints(Vector3 startPos, Vector3 startVelocity, int steps, float timeStep)
    {
        Vector3[] points = new Vector3[steps];
        
        // Get gravity value from Physics settings
        float gravity = Physics.gravity.magnitude;
        
        for (int i = 0; i < steps; i++)
        {
            float time = i * timeStep;
            
            // Calculate position at each point using projectile motion formulas
            points[i] = startPos + startVelocity * time + 0.5f * Physics.gravity * time * time;
        }
        
        return points;
    }
}