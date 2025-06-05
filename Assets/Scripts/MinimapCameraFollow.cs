using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform dice;      // Ziehe hier das Dice-Objekt rein
    public Vector3 offset = new Vector3(0, 20, 0); // Höhe und ggf. Verschiebung anpassen

    void LateUpdate()
    {
        if (dice != null)
        {
            transform.position = dice.position + offset;
        }
    }
}