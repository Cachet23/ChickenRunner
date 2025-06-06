using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;
using Map;

public class ObjectManager : MonoBehaviour
{
    [HideInInspector] public Vector2Int origin;
    [HideInInspector] public Vector2Int biomeSize;

    [Header("Required References")]
    public BaseMapManager baseMapManager;
    
    [Header("Vegetation Prefabs")]
    public GameObject[] treePrefabs;
    public GameObject highGrassPrefab;
    public GameObject lowGrassPrefab;
    public GameObject[] housePrefabs;

    [Header("Forest Settings")]
    [Range(0f, 1f)]
    public float treeDensity = 0.3f;
    public float minTreeDistance = 2f;
    public float minTreeRoadDistance = 3f;  // Mindestabstand zwischen Bäumen und Straßen
    
    [Header("Grass Settings")]
    public float highGrassRadius = 5f;
    public float lowGrassRadius = 8f;
    [Range(0f, 1f)]
    public float highGrassDensity = 0.4f;
    [Range(0f, 1f)]
    public float lowGrassDensity = 0.3f;
    
    [Header("Lake Vegetation")]
    [Range(0f, 1f)]
    public float lakeTreeChance = 0.2f;
    public float lakeEffectRadius = 6f;
    [Range(0f, 1f)]
    public float lakeHighGrassChance = 0.4f;
    [Range(0f, 1f)]
    public float lakeLowGrassChance = 0.5f;
    
    [Header("Road Vegetation")]
    public float roadGrassRadius = 3f;
    [Range(0f, 1f)]
    public float roadGrassChance = 0.3f;
    [Range(0f, 1f)]
    public float roadHighGrassRatio = 0.4f;

    [Header("House Settings")]
    public float minHouseDistance = 5f;
    
    [Header("Sorting Settings")]
    public string sortingLayerName = "Props";
    public int treeSortingOrder = 3;
    public int houseSortingOrder = 2;
    public int highGrassSortingOrder = 1;
    public int lowGrassSortingOrder = 0;

    [Header("Biome Wall")]
    public GameObject wallPrefab;
    private GameObject biomeWallInstance;

    [Header("Fog of War")]
    public GameObject fogPrefab;
    private GameObject fogInstance;

    private Transform objectContainer;
    private List<Vector3> placedObjects = new List<Vector3>();
    private Dictionary<GameObject, Vector3Int> houseDirections = new Dictionary<GameObject, Vector3Int>();

    private void Awake()
    {
        objectContainer = new GameObject("ObjectContainer").transform;
        objectContainer.parent = transform;
    }

    private void Start()
    {
        CreateFogOfWar();
        if (origin.y == 0)
            StartCoroutine(RemoveFirstBiomeFog());
    }

    private void CreateFogOfWar()
    {
        if (fogPrefab == null)
        {
            Debug.LogWarning($"[ObjectManager] Kein FogPrefab gesetzt für {name}!");
            return;
        }
        Vector3 position = new Vector3(origin.x + biomeSize.x / 2f, origin.y + biomeSize.y / 2f, -6f); // Z=5
        fogInstance = Instantiate(fogPrefab, position, Quaternion.identity, objectContainer);
        fogInstance.transform.localScale = new Vector3(biomeSize.x, biomeSize.y, 1);
        fogInstance.name = $"Fog_{origin.y / biomeSize.y}";
        fogInstance.SetActive(true);
        // Sorting Layer setzen
        var renderer = fogInstance.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = 99; // Sehr hoch, damit immer oben
        }
        Debug.Log($"[ObjectManager] Fog erzeugt für {name} an {position} mit Größe {biomeSize} und SortingLayer {sortingLayerName}");
    }

    private System.Collections.IEnumerator RemoveFirstBiomeFog()
    {
        yield return new WaitForSeconds(3f);
        RemoveFogOfWar();
    }

    public void RemoveFogOfWar()
    {
        if (fogInstance != null)
        {
            Destroy(fogInstance);
            fogInstance = null;
        }
    }

    // Public methods for MapGenerationManager to call
    public void PlaceHouses()
    {
        // Find all grass tiles first
        var bounds = baseMapManager.grassLayer.cellBounds;
        var candidates = new List<Vector3Int>();
        
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (pos.x < origin.x || pos.y < origin.y) continue;
            if (pos.x >= origin.x + biomeSize.x || pos.y >= origin.y + biomeSize.y) continue;
            if (IsValidHousePosition(baseMapManager.grassLayer.CellToWorld(pos)))
                candidates.Add(pos);
        }
        
        // Shuffle candidates for random placement
        candidates = candidates.OrderBy(x => Random.value).ToList();
        
        // Try to place houses
        int totalHouses = Mathf.Min(5, candidates.Count / 4); // Maximum 20 houses, or fewer if not enough space
        int housesPlaced = 0;
        
        foreach (var cell in candidates)
        {
            if (housesPlaced >= totalHouses) break;
            
            var worldPos = baseMapManager.grassLayer.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
            if (!placedObjects.Any(p => Vector2.Distance(p, worldPos) < minHouseDistance))
            {
                PlaceHouse(worldPos);
                housesPlaced++;
            }
        }
    }

    public void PlaceRemainingObjects()
    {
        // Stelle sicher, dass alle Platzierungen nur im eigenen Biome-Bereich erfolgen
        PlaceForestVegetation();
        PlaceLakeVegetation();
        PlaceRoadVegetation();
    }

    private bool IsInBiomeBounds(Vector3 worldPos)
    {
        // Convert 3D position to 2D grid coordinates
        var tileX = Mathf.RoundToInt(worldPos.x - 0.5f);
        var tileY = Mathf.RoundToInt(worldPos.z - 0.5f);
        
        return tileX >= origin.x && tileX < origin.x + biomeSize.x &&
               tileY >= origin.y && tileY < origin.y + biomeSize.y;
    }

    private bool IsValidHousePosition(Vector3 worldPos)
    {
        // Convert 3D position to 2D grid coordinates
        var cellPos = new Vector3Int(
            Mathf.RoundToInt(worldPos.x - 0.5f),
            Mathf.RoundToInt(worldPos.z - 0.5f),
            0
        );
        
        // Position must have grass and no water
        if (baseMapManager.grassLayer.GetTile(cellPos) == null || 
            baseMapManager.waterLayer.GetTile(cellPos) != null)
            return false;

        // Check map bounds
        if (!baseMapManager.baseLayer.cellBounds.Contains(cellPos))
            return false;

        return true;
    }



    private void PlaceHouse(Vector3 position)
    {
        var prefab = housePrefabs[Random.Range(0, housePrefabs.Length)];
        
        // Only rotate around Y axis for different house orientations
        float randomY = Random.Range(0, 4) * 90; // 0, 90, 180, or 270 degrees
        var rotation = Quaternion.Euler(0, randomY, 0);
        
        // Adjust Y position to 0 for ground level
        Vector3 adjustedPos = new Vector3(position.x, 0, position.z);
        var house = Instantiate(prefab, adjustedPos, rotation, objectContainer);

        // Set layer for houses
        house.layer = LayerMask.NameToLayer("Wall");
        foreach (Transform child in house.transform)
            child.gameObject.layer = LayerMask.NameToLayer("Wall");

        // Collider sicherstellen (kein Trigger)
        var collider = house.GetComponent<Collider>();
        if (collider == null) collider = house.AddComponent<BoxCollider>();
        collider.isTrigger = false;
        foreach (Transform child in house.transform)
        {
            var childCollider = child.GetComponent<Collider>();
            if (childCollider != null) childCollider.isTrigger = false;
        }

        var sg = house.AddComponent<SortingGroup>();
        sg.sortingLayerName = sortingLayerName;
        sg.sortingOrder = houseSortingOrder;

        // Convert rotation to grid direction
        Vector3Int direction;
        if (Mathf.Approximately(rotation.eulerAngles.y, 0f)) direction = Vector3Int.back;  // facing -Z
        else if (Mathf.Approximately(rotation.eulerAngles.y, 90f)) direction = Vector3Int.left;  // facing -X
        else if (Mathf.Approximately(rotation.eulerAngles.y, 180f)) direction = Vector3Int.forward; // facing +Z
        else direction = Vector3Int.right; // facing +X
        houseDirections[house] = direction;
        placedObjects.Add(new Vector3(position.x, 0, position.z));
    }

    public List<RoadManager.HouseFront> GetHouseFrontPositions()
    {
        var fronts = new List<RoadManager.HouseFront>();
        
        foreach (var houseEntry in houseDirections)
        {
            var house = houseEntry.Key;
            var dir = houseEntry.Value;
            var worldPos = house.transform.position;
            
            // Convert 3D position to tilemap position
            var samplePos = worldPos + new Vector3(dir.x * 0.5f, 0, dir.z * 0.5f);
            var tilePos = new Vector3Int(
                Mathf.RoundToInt(samplePos.x - 0.5f), 
                Mathf.RoundToInt(samplePos.z - 0.5f), 
                0
            );
            
            fronts.Add(new RoadManager.HouseFront(tilePos, dir));
        }
        
        return fronts;
    }

    private void PlaceForestVegetation()
    {
        foreach (var region in baseMapManager.EarthRegions)
        {
            foreach (var tile in region.Tiles)
            {
                var cellPos = baseMapManager.baseLayer.CellToWorld(tile);
                var worldPos = new Vector3(cellPos.x + 0.5f, 0f, cellPos.y + 0.5f);
                
                if (Random.value < treeDensity && 
                    !placedObjects.Any(p => Vector2.Distance(p, worldPos) < minTreeDistance) &&
                    IsValidPosition(worldPos, true))
                {
                    PlaceTree(worldPos);
                }
            }

            // Place grass around the forest
            PlaceGrassAroundRegion(region);
        }
    }

    private void PlaceLakeVegetation()
    {
        foreach (var region in baseMapManager.LakeRegions)
        {
            foreach (var tile in region.Tiles)
            {
                var cellPos = baseMapManager.waterLayer.CellToWorld(tile);
            var worldPos = new Vector3(cellPos.x + 0.5f, 0f, cellPos.y + 0.5f);
                
                for (float angle = 0; angle < 360; angle += 45)
                {
                    float rad = angle * Mathf.Deg2Rad;
                    for (float dist = 1; dist <= lakeEffectRadius; dist += 1f)
                    {
                        Vector3 checkPos = worldPos + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * dist;
                                 if (!IsValidPosition(checkPos, true)) continue;

                float distFactor = 1 - (dist / lakeEffectRadius);
                
                if (Random.value < lakeTreeChance * distFactor)
                    PlaceTree(checkPos);
                        if (Random.value < lakeHighGrassChance * distFactor)
                            PlaceGrass(checkPos, true);
                        if (Random.value < lakeLowGrassChance * distFactor)
                            PlaceGrass(checkPos, false);
                    }
                }
            }
        }
    }

    private void PlaceRoadVegetation()
    {
        var roadBounds = baseMapManager.roadLayer.cellBounds;
        foreach (var pos in roadBounds.allPositionsWithin)
        {
            if (baseMapManager.roadLayer.GetTile(pos) == null) continue;

            var cellPos = baseMapManager.roadLayer.CellToWorld(pos);
            var worldPos = new Vector3(cellPos.x + 0.5f, 0f, cellPos.y + 0.5f);
            
            for (float angle = 0; angle < 360; angle += 30)
            {
                float rad = angle * Mathf.Deg2Rad;
                for (float dist = 1; dist <= roadGrassRadius; dist += 0.5f)
                {
                    if (Random.value > roadGrassChance) continue;

                    Vector3 checkPos = worldPos + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * dist;
                    if (!IsValidPosition(checkPos)) continue;

                    PlaceGrass(checkPos, Random.value < roadHighGrassRatio);
                }
            }
        }
    }

    private void PlaceGrassAroundRegion(Map.BaseRegion region)
    {
        // Place high grass closer to the region
        foreach (var tile in region.Tiles)
        {
            var worldPos = baseMapManager.baseLayer.CellToWorld(tile) + new Vector3(0.5f, 0.5f, 0f);
            PlaceGrassInRadius(worldPos, highGrassRadius, highGrassDensity, true);
        }

        // Place low grass in a wider radius
        foreach (var tile in region.Tiles)
        {
            var worldPos = baseMapManager.baseLayer.CellToWorld(tile) + new Vector3(0.5f, 0.5f, 0f);
            PlaceGrassInRadius(worldPos, lowGrassRadius, lowGrassDensity, false);
        }
    }

    private void PlaceGrassInRadius(Vector3 center, float radius, float density, bool isHighGrass)
    {
        if (!IsInBiomeBounds(center)) return;

        for (float angle = 0; angle < 360; angle += 15)
        {
            float rad = angle * Mathf.Deg2Rad;
            for (float dist = 1; dist <= radius; dist += 0.5f)
            {
                if (Random.value > density) continue;

                Vector3 checkPos = center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * dist;
                if (!IsValidPosition(checkPos)) continue;

                PlaceGrass(checkPos, isHighGrass);
            }
        }
    }

    private void PlaceTree(Vector3 worldPos)
    {
        if (!IsInBiomeBounds(worldPos)) return;
        var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
        
        // Only rotate around Y axis, keep X and Z at 0
        float randomY = Random.Range(0, 360);
        var rotation = Quaternion.Euler(0, randomY, 0);
        
        // Adjust Y position to 0 for ground level
        Vector3 adjustedPos = new Vector3(worldPos.x, 0, worldPos.z);
        var tree = Instantiate(prefab, adjustedPos, rotation, objectContainer);

        // Set layer for trees
        tree.layer = LayerMask.NameToLayer("Wall");
        foreach (Transform child in tree.transform)
            child.gameObject.layer = LayerMask.NameToLayer("Wall");

        // Collider sicherstellen (kein Trigger)
        var collider = tree.GetComponent<Collider>();
        if (collider == null) collider = tree.AddComponent<BoxCollider>();
        collider.isTrigger = false;
        foreach (Transform child in tree.transform)
        {
            var childCollider = child.GetComponent<Collider>();
            if (childCollider != null) childCollider.isTrigger = false;
        }

        var sg = tree.AddComponent<SortingGroup>();
        sg.sortingLayerName = sortingLayerName;
        sg.sortingOrder = treeSortingOrder;
        float scale = Random.Range(0.9f, 1.1f);
        tree.transform.localScale *= scale;
        placedObjects.Add(worldPos);
    }

    private void PlaceGrass(Vector3 worldPos, bool isHighGrass)
    {
        if (!IsInBiomeBounds(worldPos)) return;
        if (placedObjects.Any(p => Vector2.Distance(new Vector2(p.x, p.z), new Vector2(worldPos.x, worldPos.z)) < minTreeDistance * 0.5f))
            return;
        var prefab = isHighGrass ? highGrassPrefab : lowGrassPrefab;
        
        // Only rotate around Y axis for variation
        float randomY = Random.Range(0, 360);
        var rotation = Quaternion.Euler(0, randomY, 0);
        
        // Adjust Y position to 0 for ground level
        Vector3 adjustedPos = new Vector3(worldPos.x, 0, worldPos.z);
        var grass = Instantiate(prefab, adjustedPos, rotation, objectContainer);

        // Setze Layer auf "IgnorePlayer" für Gras
        grass.layer = LayerMask.NameToLayer("IgnorePlayer");
        foreach (Transform child in grass.transform)
            child.gameObject.layer = LayerMask.NameToLayer("IgnorePlayer");

        // Entferne ALLE Collider-Komponenten rekursiv (auch MeshCollider, BoxCollider, etc.)
        foreach (var col in grass.GetComponents<Collider>())
            Destroy(col);
        foreach (var col in grass.GetComponentsInChildren<Collider>())
            Destroy(col);

        // Auch 2D-Collider entfernen, falls vorhanden
        foreach (var col in grass.GetComponents<Collider2D>())
            Destroy(col);
        foreach (var col in grass.GetComponentsInChildren<Collider2D>())
            Destroy(col);

        var sg = grass.AddComponent<SortingGroup>();
        sg.sortingLayerName = sortingLayerName;
        sg.sortingOrder = isHighGrass ? highGrassSortingOrder : lowGrassSortingOrder;
        float scale = Random.Range(0.8f, 1.2f);
        grass.transform.localScale *= scale;
        placedObjects.Add(worldPos);
    }
private bool IsValidPosition(Vector3 worldPos, bool isTree = false)
{
    var cellPos = baseMapManager.baseLayer.WorldToCell(worldPos);

    // Check if position is within map bounds
    if (!baseMapManager.baseLayer.cellBounds.Contains(cellPos))
        return false;

    // Check if position is in any earth region (no grass objects in earth regions)
    if (!isTree) // <--- NUR für Gras, NICHT für Bäume!
    {
        foreach (var earthRegion in baseMapManager.EarthRegions)
        {
            if (earthRegion.Tiles.Contains(cellPos))
                return false;
        }
    }

    // Check if position is not on water or road
    if (baseMapManager.waterLayer.GetTile(cellPos) != null || 
        baseMapManager.roadLayer.GetTile(cellPos) != null)
        return false;

    // For trees, check minimum distance to roads
    if (isTree)
    {
        var bounds = baseMapManager.roadLayer.cellBounds;
        for (int x = -Mathf.CeilToInt(minTreeRoadDistance); x <= Mathf.CeilToInt(minTreeRoadDistance); x++)
        {
            for (int y = -Mathf.CeilToInt(minTreeRoadDistance); y <= Mathf.CeilToInt(minTreeRoadDistance); y++)
            {
                var checkPos = cellPos + new Vector3Int(x, y, 0);
                if (bounds.Contains(checkPos) && baseMapManager.roadLayer.GetTile(checkPos) != null)
                {
                    var roadWorldPos = baseMapManager.roadLayer.CellToWorld(checkPos) + new Vector3(0.5f, 0.5f, 0f);
                    if (Vector2.Distance(worldPos, roadWorldPos) < minTreeRoadDistance)
                        return false;
                }
            }
        }
    }

    return true;
}

public void CreateBiomeWall()
{
    if (wallPrefab == null) { Debug.LogError($"[ObjectManager] wallPrefab nicht gesetzt für {name}"); return; }
    // Position: obere Grenze des Bioms
    float centerX = origin.x + (biomeSize.x / 2f) - 0.5f;
    float centerY = origin.y + biomeSize.y + 0.5f;
    Vector3 wallPosition = new Vector3(centerX, centerY, 0f);
    biomeWallInstance = Instantiate(wallPrefab, wallPosition, Quaternion.identity, objectContainer);
    biomeWallInstance.name = $"BiomeWall_{origin.y / biomeSize.y}";
    var box = biomeWallInstance.GetComponent<BoxCollider>();
    if (box == null) box = biomeWallInstance.AddComponent<BoxCollider>();
    box.isTrigger = false;
    float width = biomeSize.x, height = 1f, depth = 1f;
    biomeWallInstance.transform.localScale = new Vector3(width, height, depth);
}

public void RemoveBiomeWall()
{
    if (biomeWallInstance != null)
    {
        // Fog vom nächsten Biom entfernen
        var nextBiomeOM = FindObjectsOfType<ObjectManager>()
            .FirstOrDefault(om => om.origin.y == origin.y + biomeSize.y);
        if (nextBiomeOM != null)
            nextBiomeOM.RemoveFogOfWar();
        Destroy(biomeWallInstance);
        biomeWallInstance = null;
    }
}
}
