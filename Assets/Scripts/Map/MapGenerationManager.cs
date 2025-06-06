using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System;

public class MapGenerationManager : MonoBehaviour
{
    public event Action OnMapGenerationComplete;

    [Header("Biome ScriptableObjects")]
    public List<MapBiomeConfig> biomeConfigs;

    [Header("Globale Biomeinstellungen")]
    public Vector2Int biomeSize = new Vector2Int(50, 50); // Feste Größe für alle Biome

    [Header("Biome Wall Prefab")]
    public GameObject wallPrefab;

    [Header("Fog of War Prefab")]
    public GameObject fogPrefab;

    private List<BaseMapManager> allBMM = new List<BaseMapManager>();
    private List<ObjectManager> allOM  = new List<ObjectManager>();
    private List<TileManager> allTM    = new List<TileManager>();
    private RoadManager roadManager;

    private Tilemap FindTilemap(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) Debug.LogError($"Tilemap GameObject '{name}' not found!");
        return go ? go.GetComponent<Tilemap>() : null;
    }

    private void Start()
    {
        // Tilemaps aus der Szene suchen (einmalig, da für alle Biome gleich)
        var baseLayer = FindTilemap("BaseLayer");
        var grassLayer = FindTilemap("EarthLayer"); // oder "GrassLayer" falls vorhanden
        var waterLayer = FindTilemap("WaterLayer");
        var roadLayer = FindTilemap("RoadLayer");

        for (int i = 0; i < biomeConfigs.Count; i++)
        {
            var cfg = biomeConfigs[i];
            cfg.biomeSize = biomeSize;
            Vector2Int origin = new Vector2Int(0, i * biomeSize.y);
            var bounds = new BoundsInt(new Vector3Int(origin.x, origin.y, 0), new Vector3Int(biomeSize.x, biomeSize.y, 1));

            // BaseMapManager erzeugen
            var goB = new GameObject($"BMM_{i}"); goB.transform.parent = transform;
            var b = goB.AddComponent<BaseMapManager>();
            b.origin = origin; b.biomeSize = biomeSize;
            b.biomeBounds = bounds;
            b.baseLayer = baseLayer;
            b.grassLayer = grassLayer;
            b.waterLayer = waterLayer;
            b.roadLayer = roadLayer;
            b.earthTile = cfg.earthTile;
            b.grassTile = cfg.grassTile;
            b.waterTile = cfg.waterTile;
            b.roadTile = cfg.roadTile;
            b.noiseScale = cfg.noiseScale;
            b.useRandomSeed = cfg.useRandomSeed;
            b.seed = cfg.seed;
            b.grassThreshold = cfg.grassThreshold;
            b.waterThreshold = cfg.waterThreshold;
            b.minRegionSize = cfg.minRegionSize;
            allBMM.Add(b);

            // ObjectManager erzeugen
            var goO = new GameObject($"OM_{i}"); goO.transform.parent = transform;
            var o = goO.AddComponent<ObjectManager>();
            o.origin = origin; o.biomeSize = biomeSize; o.baseMapManager = b;
            o.treePrefabs = cfg.treePrefabs;
            o.highGrassPrefab = cfg.highGrassPrefab;
            o.lowGrassPrefab = cfg.lowGrassPrefab;
            o.housePrefabs = cfg.housePrefabs;
            o.wallPrefab = wallPrefab;
            o.fogPrefab = fogPrefab;
            o.minHouseDistance = cfg.minHouseDistance;
            o.highGrassRadius = cfg.highGrassRadius;
            o.lowGrassRadius = cfg.lowGrassRadius;
            o.treeDensity = cfg.treeDensity;
            o.lakeTreeChance = cfg.lakeTreeChance;
            o.lakeHighGrassChance = cfg.lakeHighGrassChance;
            o.lakeLowGrassChance = cfg.lakeLowGrassChance;
            o.roadGrassChance = cfg.roadGrassChance;
            o.roadHighGrassRatio = cfg.roadHighGrassRatio;
            o.sortingLayerName = cfg.sortingLayerName;
            o.treeSortingOrder = cfg.treeSortingOrder;
            o.houseSortingOrder = cfg.houseSortingOrder;
            o.highGrassSortingOrder = cfg.highGrassSortingOrder;
            o.lowGrassSortingOrder = cfg.lowGrassSortingOrder;
            allOM.Add(o);
            o.CreateBiomeWall(); // Wand für dieses Biom erzeugen

            // TileManager erzeugen
            var goT = new GameObject($"TM_{i}"); goT.transform.parent = transform;
            var t = goT.AddComponent<TileManager>();
            t.origin = origin; t.biomeSize = biomeSize; t.baseMapManager = b;
            t.biomeBounds = bounds;
            t.darkEarthTiles = cfg.darkEarthTiles;
            t.mediumEarthTiles = cfg.mediumEarthTiles;
            t.lightEarthTiles = cfg.lightEarthTiles;
            t.lightGrassTiles = cfg.lightGrassTiles;
            t.mediumGrassTiles = cfg.mediumGrassTiles;
            t.darkGrassTiles = cfg.darkGrassTiles;
            t.waterTileVariants = cfg.waterTileVariants;
            t.roadTile = cfg.roadTile;
            allTM.Add(t);
        }
        StartCoroutine(GenerateAll());
    }

    private IEnumerator GenerateAll()
    {
        foreach(var b in allBMM){ b.ValidateDependencies(); b.InitializeRandomization(); b.GenerateBaseMap(); }
        yield return null;
        foreach(var o in allOM) o.PlaceHouses(); yield return new WaitForSeconds(0.5f);
        foreach(var b in allBMM) b.DetectRegions(); yield return new WaitForSeconds(0.5f);
        // --- Region Count Check ---
        if (allBMM.Count > 1) {
            var first = allBMM[0];
            bool allEqual = true;
            foreach (var b in allBMM) {
                if (b.EarthRegions.Count != first.EarthRegions.Count ||
                    b.GrassRegions.Count != first.GrassRegions.Count ||
                    b.LakeRegions.Count != first.LakeRegions.Count) {
                    allEqual = false;
                    break;
                }
            }
            if (allEqual) {
                Debug.LogWarning("[RegionCheck] All BaseMapManager instances found the same number of regions! This likely means bounds are not set correctly.");
            } else {
                Debug.Log("[RegionCheck] Region counts differ between BaseMapManagers as expected.");
            }
        }
        foreach(var t in allTM) t.EnhanceTerrain(); yield return new WaitForSeconds(0.3f);

        // RoadManager erzeugen und alle HouseFronts übergeben
        int totalHouses = 0;
        var goR = new GameObject("RoadManager"); goR.transform.parent = transform;
        roadManager = goR.AddComponent<RoadManager>();
        roadManager.baseMapManager = allBMM[0];
        roadManager.globalOrigin = new Vector2Int(0, 0);
        roadManager.globalSize = new Vector2Int(biomeSize.x, biomeSize.y * biomeConfigs.Count);
        var allHouseFronts = new List<RoadManager.HouseFront>();
        for (int i = 0; i < allOM.Count; i++)
        {
            var houseFronts = allOM[i].GetHouseFrontPositions();
            Debug.Log($"Biome #{i}: Houses found: {houseFronts.Count}");
            foreach (var hf in houseFronts)
                Debug.Log($"Biome #{i} House at {hf.position} dir {hf.direction}");
            totalHouses += houseFronts.Count;
            allHouseFronts.AddRange(houseFronts);
        }
        Debug.Log($"Total houses for road connection: {allHouseFronts.Count}");
        foreach (var hf in allHouseFronts)
            Debug.Log($"RoadManager House at {hf.position} dir {hf.direction}");
        roadManager.SetHouseFronts(allHouseFronts);
        roadManager.enabled = true;
        yield return new WaitForSeconds(0.5f); // Warten bis Straßen platziert

        // Jetzt erst die restlichen Objekte platzieren
        foreach (var x in allOM) x.PlaceRemainingObjects(); yield return new WaitForSeconds(0.3f);

        Debug.Log("All biomes generated.");

        // Fire the event when generation is complete
        OnMapGenerationComplete?.Invoke();
    }

    // --- Am Ende der Klasse hinzufügen ---
    public List<BaseMapManager> GetAllBaseMapManagers()
    {
        return allBMM;
    }
}
