using UnityEngine;

public class WallCollisionDebug : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Wall] Kollision mit {collision.gameObject.name} um {Time.time}");
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log($"[Wall] Bleibe in Kontakt mit {collision.gameObject.name} um {Time.time}");
    }
}