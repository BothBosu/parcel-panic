using UnityEngine;
using UnityEngine.UI;

public class ParcelMarker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform parcel;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform pinImage;
    [SerializeField] private Canvas canvas;
    
    [Header("Settings")]
    [SerializeField] private float edgeBuffer = 30f;
    [SerializeField] private float maxVisibleDistance = 50f;
    [SerializeField] private float closeDistance = 5f;
    [SerializeField] private float pinOffset = 50f;
    
    [Header("Optional Elements")]
    [SerializeField] private Text distanceText;
    [SerializeField] private Color farColor = Color.white;
    [SerializeField] private Color closeColor = Color.green;
    
    private RectTransform canvasRect;
    private Image pinImageComponent;
    private float halfWidth;
    private float halfHeight;
    
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        canvasRect = canvas.GetComponent<RectTransform>();
        pinImageComponent = pinImage.GetComponent<Image>();
        
        // Calculate screen bounds
        halfWidth = canvasRect.sizeDelta.x * 0.5f;
        halfHeight = canvasRect.sizeDelta.y * 0.5f;
    }
    
    private void Update()
    {
        if (parcel == null)
            return;
        
        // Check if parcel is within a reasonable distance
        float distance = Vector3.Distance(mainCamera.transform.position, parcel.position);
        UpdateDistanceDisplay(distance);
        
        // Convert parcel position to screen coordinates
        Vector3 screenPos = mainCamera.WorldToScreenPoint(parcel.position);
        
        // If the parcel is behind the camera, flip the position
        if (screenPos.z < 0)
        {
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
            screenPos.z = 0;
        }
        
        // Check if the parcel is visible on screen
        bool isVisible = IsVisibleOnScreen(screenPos) && screenPos.z > 0;
        
        // Calculate pin position
        Vector3 pinPosition;
        
        if (isVisible)
        {
            // Parcel is visible - place pin above parcel
            pinPosition = screenPos;
            pinPosition.y += pinOffset; // Offset above the parcel
        }
        else
        {
            // Parcel is off-screen - clamp pin to screen edge
            pinPosition = ClampToScreenEdge(screenPos);
        }
        
        // Convert from screen position to canvas position
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, pinPosition, null, out anchoredPosition);
        
        // Apply position
        pinImage.anchoredPosition = anchoredPosition;
        
        // Update pin appearance based on distance
        UpdatePinAppearance(distance, isVisible);
    }
    
    private bool IsVisibleOnScreen(Vector3 screenPos)
    {
        return screenPos.x > 0 && screenPos.x < Screen.width && 
               screenPos.y > 0 && screenPos.y < Screen.height;
    }
    
    private Vector3 ClampToScreenEdge(Vector3 screenPos)
    {
        // Calculate direction from screen center to parcel
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 direction = new Vector2(screenPos.x - screenCenter.x, screenPos.y - screenCenter.y).normalized;
        
        // Calculate screen bounds with buffer
        float minX = edgeBuffer;
        float maxX = Screen.width - edgeBuffer;
        float minY = edgeBuffer;
        float maxY = Screen.height - edgeBuffer;
        
        // Find intersection with screen edge
        float t = 0;
        
        // Check horizontal edges
        if (direction.y > 0)
            t = (maxY - screenCenter.y) / direction.y;
        else if (direction.y < 0)
            t = (minY - screenCenter.y) / direction.y;
        
        // Check vertical edges
        if (direction.x > 0)
            t = Mathf.Min(t, (maxX - screenCenter.x) / direction.x);
        else if (direction.x < 0)
            t = Mathf.Min(t, (minX - screenCenter.x) / direction.x);
        
        // Calculate clamped position
        Vector2 clampedPos = screenCenter + direction * t;
        
        return new Vector3(clampedPos.x, clampedPos.y, 0);
    }
    
    private void UpdatePinAppearance(float distance, bool isVisible)
    {
        if (pinImageComponent != null)
        {
            // Adjust pin color based on distance
            if (distance <= closeDistance)
            {
                pinImageComponent.color = closeColor;
            }
            else
            {
                pinImageComponent.color = farColor;
            }
            
            // Optional: adjust pin size based on distance
            float scale = Mathf.Clamp(1f - (distance / maxVisibleDistance) * 0.5f, 0.5f, 1f);
            pinImage.localScale = new Vector3(scale, scale, 1f);
            
            // Optional: rotate pin to point toward parcel if off-screen
            if (!isVisible)
            {
                Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                Vector3 parcelScreenPos = mainCamera.WorldToScreenPoint(parcel.position);
                Vector2 direction = new Vector2(parcelScreenPos.x - screenCenter.x, parcelScreenPos.y - screenCenter.y);
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                pinImage.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                pinImage.rotation = Quaternion.identity;
            }
        }
    }
    
    private void UpdateDistanceDisplay(float distance)
    {
        if (distanceText != null)
        {
            distanceText.text = Mathf.Round(distance) + "m";
            
            // Hide distance text when very close
            distanceText.enabled = distance > closeDistance * 0.5f;
        }
    }
    
    // Public method to assign a new parcel target
    public void SetParcelTarget(Transform newParcel)
    {
        parcel = newParcel;
    }
}