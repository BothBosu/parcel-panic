using System.Collections.Generic;
using UnityEngine;

public class ParcelManager : MonoBehaviour
{
    [SerializeField] private GameObject pinPrefab;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Camera mainCamera;
    
    private List<Transform> parcels = new List<Transform>();
    private List<GameObject> pins = new List<GameObject>();
    
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }
    
    private void Start()
    {
        // You can manually add parcels in the inspector or find them by tag
        FindParcels();
        CreatePins();
    }
    
    private void FindParcels()
    {
        // Find all objects with the "Parcel" tag
        GameObject[] parcelObjects = GameObject.FindGameObjectsWithTag("Parcel");
        
        foreach (GameObject obj in parcelObjects)
        {
            parcels.Add(obj.transform);
        }
        
        Debug.Log($"Found {parcels.Count} parcels in the scene");
    }
    
    private void CreatePins()
    {
        foreach (Transform parcel in parcels)
        {
            // Instantiate a pin for each parcel
            GameObject pin = Instantiate(pinPrefab, uiCanvas.transform);
            ParcelMarker marker = pin.GetComponent<ParcelMarker>();
            
            if (marker != null)
            {
                marker.SetParcelTarget(parcel);
            }
            else
            {
                Debug.LogError("Pin prefab does not have a ParcelMarker component");
            }
            
            pins.Add(pin);
        }
    }
    
    // Add a new parcel at runtime
    public void AddParcel(Transform newParcel)
    {
        // Add to list
        parcels.Add(newParcel);
        
        // Create a pin
        GameObject pin = Instantiate(pinPrefab, uiCanvas.transform);
        ParcelMarker marker = pin.GetComponent<ParcelMarker>();
        
        if (marker != null)
        {
            marker.SetParcelTarget(newParcel);
        }
        
        pins.Add(pin);
    }
    
    // Remove a parcel at runtime
    public void RemoveParcel(Transform parcel)
    {
        int index = parcels.FindIndex(p => p == parcel);
        
        if (index >= 0)
        {
            parcels.RemoveAt(index);
            Destroy(pins[index]);
            pins.RemoveAt(index);
        }
    }
}