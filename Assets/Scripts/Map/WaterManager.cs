using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Map;

public class WaterManager : MonoBehaviour
{
    [Header("Required References")]
    public BaseMapManager baseMapManager;
    public GameObject waterPrefab;

    [HideInInspector] public Vector2Int origin;
    [HideInInspector] public Vector2Int biomeSize;

    private Transform waterContainer;

    private void Awake()
    {
        // Create water container as child of this object
        waterContainer = new GameObject("WaterContainer").transform;
        waterContainer.parent = transform;
    }

    public void SpawnWaterObjects()
    {
        if (waterPrefab == null)
        {
            Debug.LogError("WaterManager: Water prefab not assigned!");
            return;
        }

        foreach (var lakeRegion in baseMapManager.LakeRegions)
        {
            foreach (var pos in lakeRegion.Tiles)
            {
                // Convert tile position to world position
                Vector3 worldPos = baseMapManager.waterLayer.CellToWorld(pos);
                worldPos.z = 0; // Ensure Z is 0 for 2D
                
                // Instantiate water object
                GameObject waterObj = Instantiate(waterPrefab, worldPos, Quaternion.identity, waterContainer);
                waterObj.name = $"Water_{pos.x}_{pos.y}";
            }
        }
    }
}