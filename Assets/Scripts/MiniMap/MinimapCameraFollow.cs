using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform dice;      // Ziehe hier das Dice-Objekt rein
    public Vector3 offset = new Vector3(0, 20, 0); // Höhe und ggf. Verschiebung anpassen
    private Vector3 initialPosition;

    void Start()
    {
        // Speichere die initiale Position
        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        if (dice != null)
        {
            // Behalte x und y von der initialen Position, update nur z vom Würfel
            Vector3 newPosition = initialPosition;
            newPosition.z = dice.position.z + offset.z;
            transform.position = newPosition;
        }
    }
}