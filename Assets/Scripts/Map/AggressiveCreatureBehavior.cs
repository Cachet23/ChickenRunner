using UnityEngine;
using System.Collections;

public class AggressiveCreatureBehavior : CreatureBehavior
{
        private float alignmentTime = 3f;          // Time to align towards player
    private float chaseSpeed = 1.5f;           // Speed multiplier when chasing

    public void SetSettings(float alignTime, float speed)
    {
        alignmentTime = alignTime;
        chaseSpeed = speed;
    }

    private Transform playerTransform;
    private bool isAligning = false;
    private bool isChasing = false;
    private float alignmentTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        playerTransform = GameObject.FindWithTag("Dice")?.transform;
        
        if (playerTransform == null)
        {
            Debug.LogWarning("No player object with tag 'Dice' found!");
        }
    }

    protected override void Update()
    {
        if (playerTransform == null) return;

        // When enabled by the controller, start with alignment phase
        if (!isAligning && !isChasing)
        {
            StartAlignment();
        }
        else if (isAligning)
        {
            UpdateAlignment();
        }
        else if (isChasing)
        {
            ChasePlayer();
        }
    }

    private void StartAlignment()
    {
        isAligning = true;
        alignmentTimer = alignmentTime;
    }

    private void UpdateAlignment()
    {
        alignmentTimer -= Time.deltaTime;
        
        // During alignment, just look at the player
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0; // Keep it on the horizontal plane
        
        // Convert world direction to input axis for looking at player
        Vector2 input = new Vector2(directionToPlayer.x, directionToPlayer.z);
        mover.SetInput(Vector2.zero, playerTransform.position, false, false);

        if (alignmentTimer <= 0)
        {
            isAligning = false;
            isChasing = true;
        }
    }

    private void ChasePlayer()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0; // Keep it on the horizontal plane
        
        // Convert world direction to input axis
        Vector2 input = new Vector2(directionToPlayer.x, directionToPlayer.z);
        
        // Move towards player with increased speed
        mover.SetInput(input * chaseSpeed, playerTransform.position, true, false);
    }

    private void OnEnable()
    {
        // Reset state when behavior is enabled
        isAligning = false;
        isChasing = false;
    }

    private void OnDisable()
    {
        // Clean up state when behavior is disabled
        isAligning = false;
        isChasing = false;
    }
}
