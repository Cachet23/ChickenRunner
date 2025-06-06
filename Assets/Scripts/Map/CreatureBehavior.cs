using UnityEngine;
using Controller;

[RequireComponent(typeof(CreatureMover))]
public abstract class CreatureBehavior : MonoBehaviour
{
    protected CreatureMover mover;
    protected BoundsInt biomeBounds;
    protected Vector3 currentTarget;
    protected float waitTime;
    protected bool isWaiting;

    [Header("Movement Settings")]
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;
    public float moveRadius = 10f;

    protected virtual void Awake()
    {
        mover = GetComponent<CreatureMover>();
    }

    public virtual void Initialize(BoundsInt bounds)
    {
        this.biomeBounds = bounds;
        SetNewTarget();
    }

    protected virtual void Update()
    {
        if (isWaiting)
        {
            waitTime -= Time.deltaTime;
            if (waitTime <= 0)
            {
                isWaiting = false;
                SetNewTarget();
            }
            return;
        }

        // Move towards target
        Vector3 toTarget = currentTarget - transform.position;
        if (toTarget.magnitude < 0.5f)
        {
            // Reached target, wait for a bit
            isWaiting = true;
            waitTime = Random.Range(minWaitTime, maxWaitTime);
            return;
        }

        // Convert world direction to input axis
        Vector2 input = new Vector2(toTarget.x, toTarget.z).normalized;
        mover.SetInput(input, currentTarget, false, false);
    }

    protected virtual void SetNewTarget()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-moveRadius, moveRadius),
            0,
            Random.Range(-moveRadius, moveRadius)
        );

        Vector3 newTarget = transform.position + randomOffset;
        
        // Clamp to biome bounds
        newTarget.x = Mathf.Clamp(newTarget.x, biomeBounds.xMin, biomeBounds.xMax);
        newTarget.z = Mathf.Clamp(newTarget.z, biomeBounds.yMin, biomeBounds.yMax);
        
        currentTarget = newTarget;
    }
}

// Example concrete implementation
public class WanderingCreatureBehavior : CreatureBehavior
{
    // This basic implementation just uses the base behavior
    // More complex behaviors can override the base methods
}
