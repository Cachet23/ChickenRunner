using UnityEngine;
using System.Collections.Generic;

public class BehaviorController : MonoBehaviour
{
    [System.Serializable]
    public class BehaviorSettings
    {
        [Header("Basic Behavior")]
        public bool useBasicBehavior = true;
        public bool isBasicDefault = true;

        [Header("Aggressive Behavior")]
        public bool useAggressiveBehavior;
        public float aggressiveActivationDistance = 10f;
        public float alignmentTime = 3f;
        public float chaseSpeed = 1.5f;
    }

    [Header("Behavior Configuration")]
    [SerializeField] private BehaviorSettings settings = new BehaviorSettings();
    [SerializeField] private float checkInterval = 0.5f;  // How often to check distance (for performance)

    private Transform playerTransform;
    private CreatureBehavior currentBehavior;
    private float nextCheckTime;
    
    private BasicCreatureBehavior basicBehavior;
    private AggressiveCreatureBehavior aggressiveBehavior;

    private void OnValidate()
    {
        // Make sure at least one behavior is enabled
        if (!settings.useBasicBehavior && !settings.useAggressiveBehavior)
        {
            Debug.LogWarning("At least one behavior should be enabled!");
            settings.useBasicBehavior = true;
            settings.isBasicDefault = true;
        }

        // If basic behavior is not used, it can't be default
        if (!settings.useBasicBehavior)
        {
            settings.isBasicDefault = false;
        }
    }

    private void Start()
    {
        // Find player with tag "Dice"
        playerTransform = GameObject.FindWithTag("Dice")?.transform;
        
        if (playerTransform == null)
        {
            Debug.LogWarning("No player object with tag 'Dice' found!");
            return;
        }

        // Create behaviors based on settings
        if (settings.useBasicBehavior)
        {
            basicBehavior = gameObject.AddComponent<BasicCreatureBehavior>();
            basicBehavior.enabled = false;
        }

        if (settings.useAggressiveBehavior)
        {
            aggressiveBehavior = gameObject.AddComponent<AggressiveCreatureBehavior>();
            aggressiveBehavior.enabled = false;
            
            // Configure aggressive behavior settings
            if (aggressiveBehavior != null)
            {
                var aggro = aggressiveBehavior as AggressiveCreatureBehavior;
                aggro.SetSettings(settings.alignmentTime, settings.chaseSpeed);
            }
        }

        // Activate default behavior
        ActivateDefaultBehavior();
    }

    private void Update()
    {
        if (playerTransform == null || Time.time < nextCheckTime) return;

        nextCheckTime = Time.time + checkInterval;
        UpdateBehavior();
    }

    private void UpdateBehavior()
    {
        if (!settings.useAggressiveBehavior)
        {
            // If no aggressive behavior, just keep basic behavior active
            SwitchToBehavior(basicBehavior);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Switch to aggressive if player is in range
        if (distanceToPlayer <= settings.aggressiveActivationDistance)
        {
            SwitchToBehavior(aggressiveBehavior);
        }
        else
        {
            SwitchToBehavior(basicBehavior);
        }
    }

    private void ActivateDefaultBehavior()
    {
        if (settings.useBasicBehavior && settings.isBasicDefault)
        {
            SwitchToBehavior(basicBehavior);
        }
        else if (settings.useAggressiveBehavior)
        {
            SwitchToBehavior(aggressiveBehavior);
        }
    }

    private void SwitchToBehavior(CreatureBehavior newBehavior)
    {
        if (currentBehavior == newBehavior) return;

        if (currentBehavior != null)
        {
            currentBehavior.enabled = false;
        }

        if (newBehavior != null)
        {
            newBehavior.enabled = true;
            currentBehavior = newBehavior;
        }
    }

    // Optional: Visualize the activation distances in the editor
    private void OnDrawGizmosSelected()
    {
        if (settings.useAggressiveBehavior)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, settings.aggressiveActivationDistance);
        }
    }
}
