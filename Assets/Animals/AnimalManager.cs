using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class AnimalManager : MonoBehaviour
{
    public GameObject chickenPrefab;
    public int chickenCount = 5;
    public MapGenerationManager mapGenerationManager; // Referenz auf MapGenerationManager
    private BaseMapManager baseMapManager; // wird dynamisch gesetzt
    private Transform animalContainer;
    private List<GameObject> animals = new List<GameObject>();

    void Awake()
    {
        animalContainer = new GameObject("AnimalContainer").transform;
        animalContainer.parent = transform;
    }

    void Start()
    {
        // Hole den ersten BaseMapManager aus MapGenerationManager
        if (mapGenerationManager != null && mapGenerationManager.GetAllBaseMapManagers().Count > 0)
        {
            baseMapManager = mapGenerationManager.GetAllBaseMapManagers()[0];
            Debug.Log($"AnimalManager: BaseMapManager gefunden: {baseMapManager.name}");
        }
        else
        {
            Debug.LogWarning("AnimalManager: Kein BaseMapManager gefunden!");
        }
        // SpawnChickens() NICHT mehr hier aufrufen!
        Debug.Log($"AnimalManager: Tiere nach Start: {animals.Count}");
    }

    public void SpawnChickens()
    {
        // Dynamisch BaseMapManager holen
        if (mapGenerationManager != null && mapGenerationManager.GetAllBaseMapManagers().Count > 0)
            baseMapManager = mapGenerationManager.GetAllBaseMapManagers()[0];

        if (chickenPrefab == null)
        {
            Debug.LogWarning("AnimalManager: chickenPrefab ist nicht gesetzt!");
            return;
        }
        if (baseMapManager == null)
        {
            Debug.LogWarning("AnimalManager: baseMapManager ist nicht gesetzt!");
            return;
        }
        var grassTiles = new List<Vector3Int>();
        var bounds = baseMapManager.grassLayer.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (baseMapManager.grassLayer.GetTile(pos) != null)
                grassTiles.Add(pos);
        }
        Debug.Log($"AnimalManager: {grassTiles.Count} Gras-Tiles gefunden");
        for (int i = 0; i < chickenCount && grassTiles.Count > 0; i++)
        {
            int idx = Random.Range(0, grassTiles.Count);
            var cell = grassTiles[idx];
            grassTiles.RemoveAt(idx);
            Vector3 worldPos = baseMapManager.grassLayer.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
            var chicken = Instantiate(chickenPrefab, worldPos, Quaternion.Euler(-90, 0, 0), animalContainer);
            animals.Add(chicken);
            Debug.Log($"AnimalManager: Chicken an {worldPos} gespawnt");
        }
        Debug.Log($"AnimalManager: Insgesamt {animals.Count} Tiere gespawnt");
    }

    public List<GameObject> GetAllAnimals() => animals;
}
