using UnityEngine;
using UnityEngine.SceneManagement;

public class DeliveryZoneController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ParcelController parcel = other.GetComponent<ParcelController>();
        if (parcel != null)
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
