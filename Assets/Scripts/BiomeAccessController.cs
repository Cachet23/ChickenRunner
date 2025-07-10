using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Erlaubt dem Spieler, zwischen Biomen zu wechseln, indem er 3 Creatures im aktuellen Biom besiegt.
/// Entfernt die nächste vorhandene BiomeWall aus dem jeweils ersten ObjectManager, der noch eine Wand hat.
/// </summary>
public class BiomeAccessController : MonoBehaviour
{
    [Header("Würfel-Referenz (optional)")]
    [Tooltip("Transform des Würfels (Dice). Wird automatisch nach Tag \"Dice\" gesucht, falls leer gelassen.")]
    public Transform diceTransform;

    // Dictionary to track kills per biome (biomeY -> killCount)
    private Dictionary<int, int> killsPerBiome = new Dictionary<int, int>();
    private HashSet<int> processedDeaths = new HashSet<int>();
    private const int requiredKills = 3;
    private CreatureManager creatureManager;
    private MapGenerationManager mapManager;
    private bool isRemovingWall = false;

    private void Start()
    {
        // Würfel-Transform finden, falls nicht im Inspector zugewiesen
        if (diceTransform == null)
        {
            var diceGO = GameObject.FindWithTag("Dice");
            if (diceGO != null)
                diceTransform = diceGO.transform;
            else
                Debug.LogWarning("[BiomeAccessController] Kein Würfel-Objekt mit Tag \"Dice\" gefunden. Bitte im Inspector zuweisen.");
        }

        // Find required managers
        creatureManager = FindObjectOfType<CreatureManager>();
        mapManager = FindObjectOfType<MapGenerationManager>();
        
        if (creatureManager == null)
            Debug.LogError("[BiomeAccessController] CreatureManager not found in scene!");
        if (mapManager == null)
            Debug.LogError("[BiomeAccessController] MapGenerationManager not found in scene!");

        // Subscribe to OnDeath events of all existing CreatureStats
        SubscribeToCreatures();
    }

    private void SubscribeToCreatures()
    {
        Debug.Log("[BiomeAccessController] Subscribing to all creatures...");
        // Find all CreatureStats in the scene
        var allCreatures = FindObjectsByType<CreatureStats>(FindObjectsSortMode.None);
        foreach (var creature in allCreatures)
        {
            // Only subscribe if it's not the player
            if (!creature.CompareTag("Dice"))
            {
                creature.OnDeath += HandleCreatureDeath;
                Debug.Log($"[BiomeAccessController] Subscribed to {creature.gameObject.name}");
            }
        }
    }

    private void HandleCreatureDeath(CreatureStats deadCreature)
    {
        if (deadCreature == null || deadCreature.CompareTag("Dice")) return;

        // Use instance ID to track unique deaths
        int instanceId = deadCreature.gameObject.GetInstanceID();
        if (processedDeaths.Contains(instanceId))
        {
            Debug.Log($"[BiomeAccessController] Death of {deadCreature.gameObject.name} (ID: {instanceId}) already processed, skipping.");
            return;
        }

        processedDeaths.Add(instanceId);

        // Get the biome Y coordinate based on creature position
        int biomeY = Mathf.FloorToInt(deadCreature.transform.position.z); 
        // Round to nearest biome boundary (assuming biomeSize.y is 50)
        biomeY = Mathf.FloorToInt(biomeY / 50f) * 50;
        
        // Initialize or increment kill count for this biome
        if (!killsPerBiome.ContainsKey(biomeY))
        {
            killsPerBiome[biomeY] = 1;
        }
        else
        {
            killsPerBiome[biomeY]++;
        }

        Debug.Log($"[BiomeAccessController] Creature {deadCreature.gameObject.name} killed in biome {biomeY}. Current kills: {killsPerBiome[biomeY]}/{requiredKills}");

        // Check if we've reached the required kills for this biome
        if (killsPerBiome[biomeY] >= requiredKills && !isRemovingWall)
        {
            Debug.Log($"[BiomeAccessController] Required kills ({requiredKills}) reached in biome {biomeY}! Attempting to remove wall...");
            TryRemoveNextBiomeWall();
            // Reset kill count for this biome after wall is removed
            killsPerBiome[biomeY] = 0;
        }
    }

    private void OnEnable()
    {
        // Subscribe when enabled
        SubscribeToCreatures();
    }

    private void OnDisable()
    {
        // Unsubscribe from all creatures when disabled
        var allCreatures = FindObjectsByType<CreatureStats>(FindObjectsSortMode.None);
        foreach (var creature in allCreatures)
        {
            if (!creature.CompareTag("Dice"))
            {
                creature.OnDeath -= HandleCreatureDeath;
            }
        }
    }

    private void Update()
    {
        // Periodically check for new creatures that might have spawned
        if (Time.frameCount % 60 == 0) // Check every 60 frames
        {
            SubscribeToCreatures();
        }
    }

    private void TryRemoveNextBiomeWall()
    {
        if (isRemovingWall) return;
        isRemovingWall = true;

        // Hole alle ObjectManager in Szenen-Reihenfolge
        var allOM = FindObjectsByType<ObjectManager>(FindObjectsSortMode.None)
            .OrderBy(om => om.origin.y)
            .ToList();

        bool wallRemoved = false;
        foreach (var om in allOM)
        {
            var wallField = om.GetType().GetField("biomeWallInstance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var wall = wallField?.GetValue(om) as GameObject;
            if (wall != null)
            {
                om.RemoveBiomeWall();
                Debug.Log($"[BiomeAccessController] Wand von {om.name} entfernt.");
                wallRemoved = true;
                break; // Exit after removing first wall
            }
        }

        if (!wallRemoved)
        {
            Debug.Log("[BiomeAccessController] Keine weitere Biome-Wand mehr vorhanden.");
        }

        isRemovingWall = false;
    }
}