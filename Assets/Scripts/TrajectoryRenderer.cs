using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    
    [Header("Line Settings")]
    public float lineWidth = 0.1f;
    public Color lineColor = Color.white;
    public Material trajectoryMaterial; // Optional custom material
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }
    
    private void SetupLineRenderer()
    {
        // Configure the line renderer
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        
        if (trajectoryMaterial != null)
        {
            lineRenderer.material = trajectoryMaterial;
        }
        
        // Hide by default
        lineRenderer.positionCount = 0;
    }
    
    public void ShowTrajectory(Vector3[] points)
    {
        // Set the number of points
        lineRenderer.positionCount = points.Length;
        
        // Set the positions
        for (int i = 0; i < points.Length; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }
    }
    
    public void HideTrajectory()
    {
        // Hide the line renderer by setting position count to 0
        lineRenderer.positionCount = 0;
    }
}