using UnityEngine;
using Controller;

public class AnimalBehaviour : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float changeDirTime = 2f;
    private float timer;
    private Vector3 moveDir;
    private Rigidbody rb;
    private CreatureMover mover;
    private Bounds planeBounds;
    private Transform planeTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mover = GetComponent<CreatureMover>();
        PickNewDirection();

        // Suche die Plane in der Szene (Name: "Plane" oder Tag: "Ground")
        var planeObj = GameObject.Find("Plane");
        if (planeObj == null)
            planeObj = GameObject.FindGameObjectWithTag("Ground");
        if (planeObj != null)
        {
            planeTransform = planeObj.transform;
            var mesh = planeObj.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh != null)
            {
                // Bounds im lokalen Raum, auf globale Position/Rotation/Scale anwenden
                planeBounds = mesh.bounds;
            }
            else
            {
                // Fallback: nutze Collider-Bounds
                var collider = planeObj.GetComponent<Collider>();
                if (collider != null)
                    planeBounds = collider.bounds;
            }
        }
        else
        {
            Debug.LogWarning("AnimalBehaviour: Keine Plane gefunden!");
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            PickNewDirection();
    }

    void FixedUpdate()
    {
        Debug.Log($"AnimalBehaviour.FixedUpdate: moveDir={moveDir}");
        // Bewegung an CreatureMover weitergeben
        if (mover != null)
        {
            // Drehe moveDir von XY (Map) auf XZ (CharacterController)
            Vector3 ccMoveDir = Quaternion.Euler(-90, 0, 0) * moveDir;
            Debug.Log($"AnimalBehaviour: SetInput axis=({ccMoveDir.x}, {ccMoveDir.z}) target={transform.position + ccMoveDir}");
            mover.SetInput(new Vector2(ccMoveDir.x, ccMoveDir.z), transform.position + ccMoveDir, false, false);
        }
        // Falls CreatureMover nicht vorhanden, fallback auf Rigidbody
        else if (rb != null)
        {
            Vector3 velocity = new Vector3(moveDir.x, moveDir.y, 0) * moveSpeed;
            rb.linearVelocity = velocity + new Vector3(0, 0, rb.linearVelocity.z);
        }
        else
        {
            transform.position += new Vector3(moveDir.x, moveDir.y, 0) * moveSpeed * Time.fixedDeltaTime;
        }

        // Bewegung auf Plane beschränken
        if (planeTransform != null)
        {
            // Transformiere aktuelle Position in lokale Koordinaten der Plane
            Vector3 localPos = planeTransform.InverseTransformPoint(transform.position);
            // Clamp innerhalb der Plane-Bounds
            localPos.x = Mathf.Clamp(localPos.x, planeBounds.min.x, planeBounds.max.x);
            localPos.y = Mathf.Clamp(localPos.y, planeBounds.min.y, planeBounds.max.y);
            // Z bleibt (Höhe)
            // Transformiere zurück in Weltkoordinaten
            Vector3 clampedWorld = planeTransform.TransformPoint(localPos);
            transform.position = clampedWorld;
        }
    }

    void PickNewDirection()
    {
        // Random direction in X/Y
        float angle = Random.Range(0, Mathf.PI * 2);
        moveDir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        timer = changeDirTime + Random.Range(-0.5f, 0.5f);
    }
}
