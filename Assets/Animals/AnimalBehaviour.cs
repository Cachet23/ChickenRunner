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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mover = GetComponent<CreatureMover>();
        PickNewDirection();
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
    }

    void PickNewDirection()
    {
        // Random direction in X/Y
        float angle = Random.Range(0, Mathf.PI * 2);
        moveDir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        timer = changeDirTime + Random.Range(-0.5f, 0.5f);
    }
}
