using UnityEngine;
using System.Collections;

public class BasicCreatureBehavior : CreatureBehavior
{
    [Header("Movement Settings")]
    [SerializeField] private float turnSpeed = 2f;                // How quickly it turns towards target
    [SerializeField] private float acceleration = 0.5f;           // How quickly it speeds up/slows down
    [SerializeField] private float obstacleDetectionRadius = 2f;  // Radius for detecting obstacles
    [SerializeField] private LayerMask obstacleLayer;             // Layer mask for obstacles
    
    private Vector2 currentVelocity;                              // Current movement direction and speed
    private float speedMultiplier;                                // Current speed (accelerates/decelerates smoothly)
    private bool isLooking;                                      // Whether the creature is currently looking around

    protected override void Awake()
    {
        base.Awake();
        speedMultiplier = 0f;
        obstacleLayer = LayerMask.GetMask("Wall", "Default");
    }

    protected override void Update()
    {
        if (isWaiting)
        {
            HandleWaiting();
            return;
        }

        if (isLooking)
        {
            return; // Let the coroutine handle looking
        }

        // Check for water
        var tilePos = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z), 0);
        if (IsOnWater(tilePos))
        {
            SetNewTarget(); // Find new target if we're on water
            return;
        }

        Vector3 toTarget = currentTarget - transform.position;
        toTarget.y = 0f;

        // Check for obstacles and adjust direction
        if (Physics.SphereCast(transform.position, obstacleDetectionRadius, toTarget.normalized, out RaycastHit hit, 2f, obstacleLayer))
        {
            // Calculate avoidance direction
            Vector3 avoidanceDirection = Vector3.Cross(Vector3.up, hit.normal);
            currentTarget = transform.position + avoidanceDirection * moveRadius;
            toTarget = currentTarget - transform.position;
        }

        // Update movement
        if (toTarget.magnitude < 0.5f)
        {
            StartCoroutine(LookAround());
            return;
        }

        // Apply movement using the CreatureMover
        Vector2 targetVelocity = new Vector2(toTarget.x, toTarget.z).normalized;
        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, turnSpeed * Time.deltaTime);
        mover.SetInput(currentVelocity, currentTarget, false, false);
    }

    private void HandleWaiting()
    {
        speedMultiplier = Mathf.Lerp(speedMultiplier, 0f, acceleration * Time.deltaTime);
        if (speedMultiplier < 0.01f)
        {
            speedMultiplier = 0f;
            waitTime -= Time.deltaTime;
            if (waitTime <= 0)
            {
                isWaiting = false;
                StartCoroutine(LookAround());
            }
        }
    }

    private IEnumerator LookAround()
    {
        isLooking = true;
        
        // Random rotation
        float targetRotation = Random.Range(0f, 360f);
        float rotationTime = Random.Range(0.5f, 1.5f);
        float elapsed = 0f;
        
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, targetRotation, 0);
        
        while (elapsed < rotationTime)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / rotationTime);
            yield return null;
        }
        
        isLooking = false;
        SetNewTarget();
    }

    protected override void SetNewTarget()
    {
        for (int i = 0; i < 10; i++) // Try 10 times to find valid target
        {
            Vector3 randomDirection = Random.insideUnitSphere * moveRadius;
            randomDirection.y = 0;
            Vector3 newTarget = transform.position + randomDirection;
            
            // Check if target is within bounds
            newTarget.x = Mathf.Clamp(newTarget.x, biomeBounds.xMin, biomeBounds.xMax);
            newTarget.z = Mathf.Clamp(newTarget.z, biomeBounds.yMin, biomeBounds.yMax);
            
            // Check if target is not on water or road
            var targetTilePos = new Vector3Int(Mathf.RoundToInt(newTarget.x), Mathf.RoundToInt(newTarget.z), 0);
            if (!IsOnWater(targetTilePos) && !IsNearRoad(targetTilePos))
            {
                currentTarget = newTarget;
                speedMultiplier = 0f;
                return;
            }
        }
    }

    private bool IsOnWater(Vector3Int tilePos)
    {
        var waterLayer = Object.FindAnyObjectByType<BaseMapManager>(FindObjectsInactive.Exclude)?.waterLayer;
        return waterLayer != null && waterLayer.HasTile(tilePos);
    }

    private bool IsNearRoad(Vector3Int tilePos)
    {
        var roadLayer = Object.FindAnyObjectByType<BaseMapManager>(FindObjectsInactive.Exclude)?.roadLayer;
        if (roadLayer == null) return false;

        // Check surrounding tiles for roads
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (roadLayer.HasTile(tilePos + new Vector3Int(x, y, 0)))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
