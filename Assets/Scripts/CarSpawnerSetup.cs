using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CarSpawnerSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/Create Car Spawning System")]
    static void CreateCarSpawningSystem()
    {
        // Create main spawner object
        GameObject spawnerObj = new GameObject("CarSpawningSystem");
        
        // Create spawn point
        GameObject spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.SetParent(spawnerObj.transform);
        spawnPoint.transform.localPosition = new Vector3(0, 0, -50); // Start position
        
        // Create despawn point
        GameObject despawnPoint = new GameObject("DespawnPoint");
        despawnPoint.transform.SetParent(spawnerObj.transform);
        despawnPoint.transform.localPosition = new Vector3(0, 0, 50); // End position
        
        // Add visual indicators for the points (gizmos)
        spawnPoint.AddComponent<SpawnPointVisualizer>().pointType = SpawnPointVisualizer.PointType.Spawn;
        despawnPoint.AddComponent<SpawnPointVisualizer>().pointType = SpawnPointVisualizer.PointType.Despawn;
        
        // Add the car spawner component to the main object
        CarSpawner spawner = spawnerObj.AddComponent<CarSpawner>();
        spawner.spawnPoint = spawnPoint.transform;
        spawner.despawnPoint = despawnPoint.transform;
        
        // Select the spawner object in the hierarchy
        Selection.activeGameObject = spawnerObj;
        
        Debug.Log("Car Spawning System created! Now add your car prefabs to the CarSpawner component.");
    }
#endif
}

// Helper class to visualize spawn and despawn points in the editor
public class SpawnPointVisualizer : MonoBehaviour
{
    public enum PointType { Spawn, Despawn }
    public PointType pointType = PointType.Spawn;
    
    private void OnDrawGizmos()
    {
        // Set color based on point type
        Gizmos.color = pointType == PointType.Spawn ? Color.green : Color.red;
        
        // Draw a sphere at the point's position
        Gizmos.DrawSphere(transform.position, 1f);
        
        // Draw an arrow indicating direction
        Vector3 arrowDirection = pointType == PointType.Spawn ? transform.forward : -transform.forward;
        Gizmos.DrawRay(transform.position, arrowDirection * 5f);
    }
}