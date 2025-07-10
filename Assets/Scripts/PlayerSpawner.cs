using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerCreature;  // Reference to the player creature in scene
    [SerializeField] private GameObject roof;           // Reference to the roof plane

    [Header("Spawn Settings")]
    [SerializeField] private float minDistanceFromTrees = 2f;
    [SerializeField] private float minDistanceFromWater = 3f;  // Minimum distance from water tiles
    [SerializeField] private LayerMask treeLayer;       // Layer containing trees

    private MapGenerationManager mapManager;
    private BaseMapManager firstBiome;    // We'll spawn in the first biome

    private void Start()
    {
        // Find required components
        mapManager = FindObjectOfType<MapGenerationManager>();
        if (mapManager == null)
        {
            Debug.LogError("PlayerSpawner: Could not find MapGenerationManager!");
            return;
        }

        // Subscribe to map generation complete event
        mapManager.OnMapGenerationComplete += OnMapGenerationComplete;

        // Validate references
        if (playerCreature == null)
        {
            Debug.LogError("PlayerSpawner: Player Creature reference not set!");
            return;
        }

        if (roof == null)
        {
            Debug.LogError("PlayerSpawner: Roof reference not set!");
            return;
        }
    }

    private void OnMapGenerationComplete()
    {
        // Get the first biome manager
        var allBMMs = mapManager.GetAllBaseMapManagers();
        if (allBMMs.Count == 0)
        {
            Debug.LogError("PlayerSpawner: No biome managers found!");
            return;
        }

        firstBiome = allBMMs[0];
        Vector3? spawnPosition = FindValidSpawnPosition();

        if (spawnPosition.HasValue)
        {
            // Move player high above the found position first
            Vector3 highPosition = spawnPosition.Value;
            highPosition.y = 30f;  // Place it at y=30
            playerCreature.transform.position = highPosition;
            
            // Remove the roof
            Destroy(roof);
            
            Debug.Log($"PlayerSpawner: Successfully spawned player above position {highPosition}");
        }
        else
        {
            Debug.LogError("PlayerSpawner: Could not find valid spawn position!");
        }

        // Unsubscribe from the event
        mapManager.OnMapGenerationComplete -= OnMapGenerationComplete;
    }

    private Vector3? FindValidSpawnPosition()
    {
        // Start from z > 0 and move towards higher z values
        for (int z = 1; z < firstBiome.biomeSize.y; z++)
        {
            for (int x = 0; x < firstBiome.biomeSize.x; x++)
            {
                Vector3Int tilePos = new Vector3Int(x, z, 0);
                Vector3 worldPos = firstBiome.grassLayer.CellToWorld(tilePos);
                worldPos.y = -22f; // Set to ground level

                // Check if position is valid
                if (IsValidSpawnPosition(tilePos, worldPos))
                {
                    return worldPos;
                }
            }
        }

        return null;
    }

    private bool IsValidSpawnPosition(Vector3Int tilePos, Vector3 worldPos)
    {
        // Must be on grass tile
        if (!firstBiome.grassLayer.HasTile(tilePos))
            return false;

        // Check for water tiles in radius
        for (int dx = -Mathf.CeilToInt(minDistanceFromWater); dx <= Mathf.CeilToInt(minDistanceFromWater); dx++)
        {
            for (int dy = -Mathf.CeilToInt(minDistanceFromWater); dy <= Mathf.CeilToInt(minDistanceFromWater); dy++)
            {
                Vector3Int checkPos = tilePos + new Vector3Int(dx, dy, 0);
                if (firstBiome.waterLayer.HasTile(checkPos))
                {
                    // Calculate actual distance to this water tile
                    Vector3 waterWorldPos = firstBiome.waterLayer.CellToWorld(checkPos);
                    waterWorldPos.y = worldPos.y; // Use same Y for distance calculation
                    if (Vector3.Distance(worldPos, waterWorldPos) <= minDistanceFromWater)
                    {
                        return false;
                    }
                }
            }
        }

        // Check for nearby trees
        Collider[] nearbyTrees = Physics.OverlapSphere(worldPos, minDistanceFromTrees, treeLayer);
        if (nearbyTrees.Length > 0)
            return false;

        // Check for nearby water
        Collider[] nearbyWater = Physics.OverlapSphere(worldPos, minDistanceFromWater, treeLayer);
        if (nearbyWater.Length > 0)
            return false;

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && firstBiome != null)
        {
            Gizmos.color = Color.green;
            // Draw search area
            Vector3 center = firstBiome.grassLayer.CellToWorld(new Vector3Int(
                firstBiome.biomeSize.x / 2,
                firstBiome.biomeSize.y / 2,
                0
            ));
            Gizmos.DrawWireCube(center, new Vector3(
                firstBiome.biomeSize.x,
                1,
                firstBiome.biomeSize.y
            ));
        }
    }
}
