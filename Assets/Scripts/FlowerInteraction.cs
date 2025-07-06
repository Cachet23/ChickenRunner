using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class FlowerInteraction : MonoBehaviour
{
    // Static tracking of all flowers in range of player
    private static readonly List<FlowerInteraction> flowersInRange = new List<FlowerInteraction>();
    private static FlowerInteraction activeFlower;

    [Header("Flower Properties")]
    [SerializeField] private FlowerConfig.Rarity rarity;

    [Header("Creature Stat Effects")]
    [Tooltip("Amount of health to restore. Set to 0 for no effect.")]
    [SerializeField] private float healthRestoreAmount = 0f;
    
    [Tooltip("Amount of stamina to restore. Set to 0 for no effect.")]
    [SerializeField] private float staminaRestoreAmount = 0f;
    
    [Tooltip("Amount of mana to restore. Set to 0 for no effect.")]
    [SerializeField] private float manaRestoreAmount = 0f;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private GameObject interactionUIPrefab;
    
    private GameObject activeUI;
    private Transform player;
    private bool isInRange;
    private float distanceToPlayer;

    public FlowerConfig.Rarity Rarity
    {
        get => rarity;
        set
        {
            rarity = value;
            Debug.Log($"[FlowerInteraction] Set rarity to {value} on {gameObject.name}");
        }
    }

    private void OnEnable()
    {
        flowersInRange.Add(this);
        UpdateActiveFlower();
    }

    private void OnDisable()
    {
        flowersInRange.Remove(this);
        if (activeFlower == this)
        {
            HideInteractionUI();
            activeFlower = null;
        }
        UpdateActiveFlower();
    }

    private void Start()
    {
        // Find the player (assuming it has the Dice tag)
        player = GameObject.FindWithTag("Dice")?.transform;
        if (player == null)
        {
            Debug.LogWarning("[FlowerInteraction] Player not found! Make sure it has the 'Dice' tag.");
            return;
        }

        Debug.Log($"[FlowerInteraction] Initialized flower on {gameObject.name} with rarity {rarity}");
    }

    private void Update()
    {
        if (player == null) return;

        // Check if player is in range
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool wasInRange = isInRange;
        isInRange = distanceToPlayer <= interactionRadius;

        // Handle range changes
        if (isInRange != wasInRange)
        {
            UpdateActiveFlower();
        }

        // Handle interaction input only if we're the active flower
        if (isInRange && activeFlower == this && Input.GetKeyDown(KeyCode.E))
        {
            EatFlower();
        }
    }

    private static void UpdateActiveFlower()
    {
        // Find the nearest flower in range
        var nearestFlower = flowersInRange
            .Where(f => f.isInRange)
            .OrderBy(f => f.distanceToPlayer)
            .FirstOrDefault();

        // If the active flower has changed
        if (activeFlower != nearestFlower)
        {
            // Hide UI of previous active flower
            if (activeFlower != null)
            {
                activeFlower.HideInteractionUI();
            }

            // Show UI of new active flower
            activeFlower = nearestFlower;
            if (activeFlower != null)
            {
                activeFlower.ShowInteractionUI();
            }
        }
    }

    private void ShowInteractionUI()
    {
        if (activeUI != null) return;

        // Create UI at flower position
        activeUI = Instantiate(interactionUIPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
        
        // Update UI text based on rarity and effects
        if (activeUI.GetComponentInChildren<TextMeshProUGUI>() is TextMeshProUGUI tmp)
        {
            string rarityColor = rarity switch
            {
                FlowerConfig.Rarity.Common => "white",
                FlowerConfig.Rarity.Rare => "cyan",
                FlowerConfig.Rarity.Epic => "magenta",
                _ => "white"
            };

            string effectsText = GetEffectsDescription();
            tmp.text = $"Press E to collect <color={rarityColor}>{rarity}</color> flower\n{effectsText}";
        }
        
        Debug.Log($"[FlowerInteraction] Showing UI for {rarity} flower");
    }

    private string GetEffectsDescription()
    {
        var effects = new System.Collections.Generic.List<string>();
        
        if (healthRestoreAmount > 0)
            effects.Add($"+{healthRestoreAmount} Health");
        if (staminaRestoreAmount > 0)
            effects.Add($"+{staminaRestoreAmount} Stamina");
        if (manaRestoreAmount > 0)
            effects.Add($"+{manaRestoreAmount} Mana");

        return string.Join(", ", effects);
    }

    private void HideInteractionUI()
    {
        if (activeUI != null)
        {
            Destroy(activeUI);
            activeUI = null;
        }
    }

    private void EatFlower()
    {
        if (player.GetComponent<CreatureStats>() is CreatureStats playerStats)
        {
            bool anyEffectApplied = false;

            // Apply all configured effects
            if (healthRestoreAmount > 0)
            {
                playerStats.RestoreHealth(healthRestoreAmount);
                anyEffectApplied = true;
                Debug.Log($"[FlowerInteraction] Restored {healthRestoreAmount} health");
            }

            if (staminaRestoreAmount > 0)
            {
                playerStats.RestoreStamina(staminaRestoreAmount);
                anyEffectApplied = true;
                Debug.Log($"[FlowerInteraction] Restored {staminaRestoreAmount} stamina");
            }

            if (manaRestoreAmount > 0)
            {
                playerStats.RestoreMana(manaRestoreAmount);
                anyEffectApplied = true;
                Debug.Log($"[FlowerInteraction] Restored {manaRestoreAmount} mana");
            }

            if (!anyEffectApplied)
            {
                Debug.LogWarning("[FlowerInteraction] Flower consumed but no effects were configured!");
            }
        }
        else
        {
            Debug.LogError("[FlowerInteraction] Player missing CreatureStats component!");
        }

        // Clean up and destroy the flower
        HideInteractionUI();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
