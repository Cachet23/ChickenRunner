using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Configuration")]
    public List<MapBiomeConfig> biomeConfigs;

    [Header("Global Prefabs")]
    [Tooltip("Prefab for the walls between biomes")]
    public GameObject wallPrefab;
    [Tooltip("Prefab for the fog of war")]
    public GameObject fogPrefab;

    private MapGenerationManager mapManager;
    private CreatureManager creatureManager;

    private void Awake()
    {
        // Create MapGenerationManager
        var mapManagerGO = new GameObject("MapGenerationManager");
        mapManagerGO.transform.parent = transform;
        mapManager = mapManagerGO.AddComponent<MapGenerationManager>();
        mapManager.biomeConfigs = biomeConfigs;
        mapManager.wallPrefab = wallPrefab;
        mapManager.fogPrefab = fogPrefab;

        // Create CreatureManager
        var creatureManagerGO = new GameObject("CreatureManager");
        creatureManagerGO.transform.parent = transform;
        creatureManager = creatureManagerGO.AddComponent<CreatureManager>();

        // Create container for creatures
        var creatureContainer = new GameObject("CreatureContainer");
        creatureContainer.transform.parent = creatureManagerGO.transform;
    }

    private void Start()
    {
        // Subscribe to map generation complete event
        mapManager.OnMapGenerationComplete += OnMapGenerationComplete;
    }

    private void OnMapGenerationComplete()
    {
        // Initialize creature manager with map data and configs
        creatureManager.Initialize(mapManager, biomeConfigs);
    }
}
