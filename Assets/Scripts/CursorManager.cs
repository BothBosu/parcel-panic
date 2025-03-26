using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        // Confine the cursor to the game window
        Cursor.lockState = CursorLockMode.Confined;
        // Make sure the cursor is visible if needed
        Cursor.visible = true;
    }
}
