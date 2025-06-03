using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DiceController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;

    void Awake()
    {
        // Rigidbody holen oder neu hinzufügen
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        // BoxCollider holen oder neu hinzufügen
        var box = GetComponent<BoxCollider>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = false;

        // Rotation und Z-Position einfrieren
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

        // Schwerkraft deaktivieren, damit er nicht herunterfällt
        rb.useGravity = false;

        // Kinematisch = false belassen, damit Kollisionen mit statischen Collidern (Wand) wirken
        rb.isKinematic = false;
    }

    void FixedUpdate()
    {
        // Input abfragen
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector3 move = new Vector3(moveX, moveY, 0f).normalized;

        if (move.sqrMagnitude > 0.01f)
        {
            rb.linearVelocity = move * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
}