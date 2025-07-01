using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CreatureManager : MonoBehaviour
{
    // Track all spawned creatures for removal
    private readonly List<GameObject> spawnedCreatures = new List<GameObject>();
    private MapGenerationManager mapManager;
    private List<MapBiomeConfig> biomeConfigs;
    private Transform creatureContainer;

    private void Awake()
    {
        creatureContainer = transform.Find("CreatureContainer");
        if (creatureContainer == null)
        {
            creatureContainer = new GameObject("CreatureContainer").transform;
            creatureContainer.parent = transform;
        }
    }

    public void Initialize(MapGenerationManager mapManager, List<MapBiomeConfig> configs)
    {
        this.mapManager = mapManager;
        this.biomeConfigs = configs;
        SpawnCreatures();
    }

    private void SpawnCreatures()
    {
        var allBMMs = mapManager.GetAllBaseMapManagers();
        Debug.Log($"Spawning creatures for {biomeConfigs.Count} biomes...");
        
        // Spawn creatures for each biome
        for (int i = 0; i < biomeConfigs.Count && i < allBMMs.Count; i++)
        {
            var config = biomeConfigs[i];
            var bmm = allBMMs[i];
            
            Debug.Log($"Biome {i}: Found {config.creatures.Count} creature types to spawn");
            foreach (var creatureConfig in config.creatures)
            {
                if (creatureConfig.creaturePrefab == null)
                {
                    Debug.LogError($"Creature prefab is null in biome {i}");
                    continue;
                }
                
                Debug.Log($"Spawning {creatureConfig.spawnCount}x {creatureConfig.creaturePrefab.name} in biome {i}");
                for (int j = 0; j < creatureConfig.spawnCount; j++)
                {
                    SpawnCreature(creatureConfig.creaturePrefab, bmm);
                }
            }
        }
    }

    private void SpawnCreature(GameObject creaturePrefab, BaseMapManager bmm)
    {
        Debug.Log($"Attempting to spawn {creaturePrefab.name} in bounds: x({bmm.biomeBounds.xMin}-{bmm.biomeBounds.xMax}), y({bmm.biomeBounds.yMin}-{bmm.biomeBounds.yMax})");
        
        // Try to find a valid spawn position
        for (int attempts = 0; attempts < 20; attempts++) // Increased attempts to 20
        {
            // Get random position within biome bounds
            int x = Random.Range(bmm.biomeBounds.xMin, bmm.biomeBounds.xMax);
            int y = Random.Range(bmm.biomeBounds.yMin, bmm.biomeBounds.yMax);
            // Place the creature higher above ground to ensure it doesn't intersect
            Vector3 position = new Vector3(x, 2f, y);

            Debug.Log($"Attempt {attempts + 1}: Testing position {position}");

            // Check if position is valid (on earth/grass, not on objects)
            if (IsValidSpawnPosition(position, bmm))
            {
                Debug.Log($"Found valid position at {position}");
                var creature = Instantiate(creaturePrefab, position, Quaternion.identity, creatureContainer);
                
                if (creature == null)
                {
                    Debug.LogError($"Failed to instantiate creature!");
                    continue;
                }
                
                Debug.Log($"Successfully instantiated {creature.name} at {position}");
                
                // Ensure every creature has CreatureStats
                var stats = creature.GetComponent<CreatureStats>();
                if (stats == null)
                {
                    stats = creature.AddComponent<CreatureStats>();
                }

                // Subscribe to death event
                stats.OnDeath += HandleCreatureDeath;

                // Track creature
                spawnedCreatures.Add(creature);

                var behavior = creature.GetComponent<CreatureBehavior>();
                if (behavior != null)
                {
                    behavior.Initialize(bmm.biomeBounds);
                    Debug.Log($"Initialized behavior for {creature.name}");
                }
                return; // Successfully spawned
            }
            Debug.Log($"Position {position} is not valid");
        }
        Debug.LogWarning($"Failed to find valid spawn position for {creaturePrefab.name} after 20 attempts");
    }

    // Remove creature when it dies
    private void HandleCreatureDeath(CreatureStats stats)
    {
        if (stats != null)
        {
            GameObject go = stats.gameObject;
            spawnedCreatures.Remove(go);
            Destroy(go);
        }
    }

    private bool IsValidSpawnPosition(Vector3 position, BaseMapManager bmm)
    {
        // Convert world position to tilemap position, ignoring the y component for tile checks
        var worldPos = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z), 0);
        
        // Check for water tiles - return false immediately if water is found
        if (bmm.waterLayer.HasTile(worldPos))
        {
            Debug.Log($"Position {position} has water - spawn rejected");
            return false;
        }

        // Check surrounding tiles for water to prevent spawning right at water edges
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                var checkPos = worldPos + new Vector3Int(x, y, 0);
                if (bmm.waterLayer.HasTile(checkPos))
                {
                    Debug.Log($"Position {position} is too close to water - spawn rejected");
                    return false;
                }
            }
        }

        // Check if there's earth/grass at this position
        bool hasGround = bmm.baseLayer.HasTile(worldPos) ||
                        bmm.grassLayer.HasTile(worldPos);

        if (!hasGround)
        {
            Debug.Log($"Position {position} has no ground");
            return false;
        }

        // Check for obstacles using a sphere cast from above
        RaycastHit[] hits = Physics.SphereCastAll(
            position + Vector3.up * 10f,
            0.5f,
            Vector3.down,
            20f
        );

        var obstacles = hits.Where(hit => 
            !hit.collider.isTrigger &&
            hit.collider.gameObject.layer != LayerMask.NameToLayer("Default")
        ).ToArray();

        if (obstacles.Length > 0)
        {
            Debug.Log($"Position {position} has {obstacles.Length} obstacles");
            return false;
        }

        return true;
    }
}
