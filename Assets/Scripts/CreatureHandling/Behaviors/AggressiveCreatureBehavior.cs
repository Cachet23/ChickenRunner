using UnityEngine;
using System.Collections;

public class AggressiveCreatureBehavior : CreatureBehavior
{
    private float alignmentTime = 3f;          // Time to align towards player
    private float chaseSpeed = 1.5f;           // Speed multiplier when chasing
    private const float STAMINA_SPRINT_THRESHOLD = 0.95f;  // Warte bis 95% Stamina zum erneuten Sprint
    private const float STAMINA_EXHAUSTED_THRESHOLD = 0.05f;  // Bei 5% Stamina ist erschöpft

    private Transform playerTransform;
    private bool isAligning = false;
    private bool isChasing = false;
    private float alignmentTimer = 0f;
    private CreatureStats myStats;
    private bool canSprint = true;

    public void SetSettings(float alignTime, float speed)
    {
        alignmentTime = alignTime;
        chaseSpeed = speed;
    }

    protected override void Awake()
    {
        base.Awake();
        playerTransform = GameObject.FindWithTag("Dice")?.transform;
        myStats = GetComponent<CreatureStats>();
        
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
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (myStats == null)
        {
            Debug.LogWarning("[AggressiveCreatureBehavior] Missing CreatureStats component!");
            return;
        }

        // Stamina Management
        float currentStaminaPercent = myStats.GetStaminaPercent();
        
        // Wenn erschöpft, warte auf fast volle Regeneration
        if (!canSprint && currentStaminaPercent >= STAMINA_SPRINT_THRESHOLD)
        {
            canSprint = true;
            Debug.Log($"[AggressiveCreatureBehavior] {gameObject.name} Stamina voll regeneriert ({currentStaminaPercent:P0}), beginne Sprint");
        }
        // Wenn Stamina fast leer, stoppe Sprint und warte auf Regeneration
        else if (canSprint && currentStaminaPercent <= STAMINA_EXHAUSTED_THRESHOLD)
        {
            canSprint = false;
            Debug.Log($"[AggressiveCreatureBehavior] {gameObject.name} Stamina erschöpft ({currentStaminaPercent:P0}), regeneriere...");
        }

        var playerStats = playerTransform.GetComponent<CreatureStats>();
        if (playerStats == null) return;

        // Attack range check (subtract 1 to maintain some distance)
        float targetDistance = myStats.AttackRange - 1f;
        
        if (distanceToPlayer <= myStats.AttackRange)
        {
            // In attack range - try to attack
            if (myStats.TryAttack(playerStats))
            {
                // Attacke war erfolgreich - bleib stehen
                mover.SetInput(Vector2.zero, playerTransform.position, false, false);
                return;
            }
        }
        
        // Not in range or can't attack yet - keep chasing if we're too far
        if (distanceToPlayer > targetDistance)
        {
            Vector2 input = new Vector2(directionToPlayer.x, directionToPlayer.z);
            bool shouldRun = canSprint && distanceToPlayer > myStats.AttackRange * 1.5f; // Sprint nur wenn weiter weg
            mover.SetInput(input * (shouldRun ? chaseSpeed : 1f), playerTransform.position, shouldRun, false);
        }
        else
        {
            // In ideal range - stop moving
            mover.SetInput(Vector2.zero, playerTransform.position, false, false);
        }
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
