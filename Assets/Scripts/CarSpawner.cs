using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("Car Prefabs")]
    [Tooltip("List of car prefabs to spawn randomly")]
    public GameObject[] carPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Position where cars will spawn")]
    public Transform spawnPoint;
    
    [Tooltip("Position where cars will be destroyed after reaching")]
    public Transform despawnPoint;
    
    [Tooltip("Minimum time between car spawns")]
    public float minSpawnInterval = 1.5f;
    
    [Tooltip("Maximum time between car spawns")]
    public float maxSpawnInterval = 4.0f;
    
    [Tooltip("Minimum speed for cars")]
    public float minSpeed = 5.0f;
    
    [Tooltip("Maximum speed for cars")]
    public float maxSpeed = 10.0f;
    
    [Header("Lane Settings")]
    [Tooltip("Number of lanes to spawn cars in")]
    public int laneCount = 3;
    
    [Tooltip("Distance between lanes")]
    public float laneWidth = 3.0f;

    // Start spawning when the script is enabled
    private void OnEnable()
    {
        StartCoroutine(SpawnCars());
    }

    // Stop spawning when the script is disabled
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    // Coroutine for spawning cars
    IEnumerator SpawnCars()
    {
        while (true)
        {
            // Spawn a car
            SpawnCar();
            
            // Wait for a random time before spawning the next car
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    // Spawn a single car
    void SpawnCar()
    {
        // Select a random car prefab
        int carIndex = Random.Range(0, carPrefabs.Length);
        
        // Select a random lane
        int lane = Random.Range(0, laneCount);
        
        // Calculate spawn position with lane offset
        Vector3 spawnPosition = spawnPoint.position;
        Vector3 laneOffset = spawnPoint.right * ((lane - (laneCount - 1) / 2.0f) * laneWidth);
        spawnPosition += laneOffset;

        
        // Create the car
        GameObject car = Instantiate(carPrefabs[carIndex], spawnPosition, spawnPoint.rotation);
        
        // Add a car controller to the new car
        float speed = Random.Range(minSpeed, maxSpeed);
        car.AddComponent<CarController>().Initialize(speed, despawnPoint);
    }
}

// Controller script for individual cars
public class CarController : MonoBehaviour
{
    private float speed;
    private Transform targetPoint;

    // Initialize the car with speed and target point
    public void Initialize(float carSpeed, Transform despawnPoint)
    {
        speed = carSpeed;
        targetPoint = despawnPoint;
    }

    // Move the car forward each frame
    private void Update()
{
    // Move forward at the assigned speed
    transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);

    // Check distance to despawn point
    if (Vector3.Distance(transform.position, targetPoint.position) <= 5.0f)
    {
        Destroy(gameObject);
        return;
    }

    // Check if the car has passed the despawn point by using the dot product
    Vector3 toTarget = targetPoint.position - transform.position;
    // If the dot product is less than zero, it means the car's forward direction is away from the target
    if (Vector3.Dot(transform.forward, toTarget) < 0)
    {
        Destroy(gameObject);
    }
}

}