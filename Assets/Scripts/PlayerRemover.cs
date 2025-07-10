using UnityEngine;
using Controller;

public class PlayerRemover : MonoBehaviour
{
    [Header("Death Animation Settings")]
    [SerializeField] private float rotationSpeed = 90f;       // Degrees per second
    [SerializeField] private float ascensionSpeed = 2f;      // Units per second
    [SerializeField] private float startDelay = 1f;          // Delay before starting animation

    private CreatureStats playerStats;
    private MovePlayerInput playerInput;
    private CreatureMover creatureMover;
    private CharacterController characterController;
    private bool isAnimating = false;
    private float deathTimer = 0f;
    private bool isDead = false;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Dice");
        if (player == null)
        {
            Debug.LogError("PlayerRemover: Could not find player with tag 'Dice'!");
            return;
        }

        playerStats = player.GetComponent<CreatureStats>();
        playerInput = player.GetComponent<MovePlayerInput>();
        creatureMover = player.GetComponent<CreatureMover>();
        characterController = player.GetComponent<CharacterController>();

        if (playerStats != null)
        {
            // Subscribe to health changes
            playerStats.OnHealthChanged += CheckHealth;
        }
        else
        {
            Debug.LogError("PlayerRemover: Player does not have CreatureStats component!");
        }
    }

    private void CheckHealth(float healthPercent)
    {
        if (healthPercent <= 0 && !isDead)
        {
            StartDeathSequence();
        }
    }

    private void StartDeathSequence()
    {
        if (isDead) return;
        isDead = true;

        // Disable all movement and control components
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }
        if (creatureMover != null)
        {
            creatureMover.enabled = false;  // Dies stoppt auch die CharacterController Bewegungen
        }
        if (playerStats != null)
        {
            playerStats.enabled = false;
        }

        // Start animation sequence
        isAnimating = true;
        deathTimer = 0f;
        
        Debug.Log("PlayerRemover: Starting death sequence...");
    }

    private void Update()
    {
        if (!isAnimating) return;

        deathTimer += Time.deltaTime;

        // Get reference to transform once per frame
        Transform playerTransform = playerStats.transform;

        // Rotate around Z axis (starts immediately)
        float currentRotation = playerTransform.rotation.eulerAngles.z;
        if (currentRotation < 90f)
        {
            float newRotation = Mathf.MoveTowards(currentRotation, 90f, rotationSpeed * Time.deltaTime);
            playerTransform.rotation = Quaternion.Euler(0f, 0f, newRotation);
        }

        // Wait for start delay before moving upward
        if (deathTimer >= startDelay)
        {
            // Move upward only after delay
            Vector3 position = playerTransform.position;
            position.y += ascensionSpeed * Time.deltaTime;
            playerTransform.position = position;
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= CheckHealth;
        }
    }
}
