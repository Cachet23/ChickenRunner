using UnityEngine;
using System.Collections;

public class BasicCreatureBehavior : CreatureBehavior
{
    [Header("Peaceful Movement")]
    [SerializeField] private float turnSpeed = 2f;        // How quickly it turns towards target
    [SerializeField] private float acceleration = 0.5f;   // How quickly it speeds up/slows down
    
    private Vector2 currentVelocity;                      // Current movement direction and speed
    private float speedMultiplier;                        // Current speed (accelerates/decelerates smoothly)

    protected override void Awake()
    {
        base.Awake();
        speedMultiplier = 0f;
        
        // Adjust base class values for more peaceful movement
        minWaitTime = 3f;     // Wait longer between movements
        maxWaitTime = 8f;     // Maximum wait time increased
        moveRadius = 15f;     // Larger radius for more natural wandering
    }

    protected override void Update()
    {
        if (isWaiting)
        {
            // Smoothly decelerate while waiting
            speedMultiplier = Mathf.Lerp(speedMultiplier, 0f, acceleration * Time.deltaTime);
            if (speedMultiplier < 0.01f)
            {
                speedMultiplier = 0f;
                base.Update();
                return;
            }
        }
        else
        {
            // Smoothly accelerate while moving
            speedMultiplier = Mathf.Lerp(speedMultiplier, 1f, acceleration * Time.deltaTime);
        }

        // Calculate direction to target
        Vector3 toTarget = currentTarget - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude < 0.5f)
        {
            isWaiting = true;
            waitTime = Random.Range(minWaitTime, maxWaitTime);
            return;
        }

        // Smoothly interpolate current velocity towards target direction
        Vector2 targetVelocity = new Vector2(toTarget.x, toTarget.z).normalized;
        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, turnSpeed * Time.deltaTime);

        // Apply smooth movement using the CreatureMover
        mover.SetInput(currentVelocity * speedMultiplier, currentTarget, false, false);
    }

    protected override void SetNewTarget()
    {
        // Add slight randomness to timing
        minWaitTime = Random.Range(3f, 4f);
        maxWaitTime = Random.Range(7f, 9f);

        base.SetNewTarget();

        // Start with zero speed when setting new target
        speedMultiplier = 0f;
    }
}
