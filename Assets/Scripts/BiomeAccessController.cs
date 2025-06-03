using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Für OrderBy

/// <summary>
/// Erlaubt dem Spieler, zwischen Biomen zu wechseln, indem er 3× die Leertaste in 3 Sekunden drückt.
/// Entfernt die nächste vorhandene BiomeWall aus dem jeweils ersten ObjectManager, der noch eine Wand hat.
/// </summary>
public class BiomeAccessController : MonoBehaviour
{
    [Header("Würfel-Referenz (optional)")]
    [Tooltip("Transform des Würfels (Dice). Wird automatisch nach Tag \"Dice\" gesucht, falls leer gelassen.")]
    public Transform diceTransform;

    // Challenge: 3× Leertaste innerhalb von 3 Sekunden
    private const int requiredPresses = 3;
    private const float timeWindow = 3f;
    private List<float> spaceTimestamps = new List<float>();

    void Start()
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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float now = Time.time;
            spaceTimestamps.Add(now);

            // Entferne alte Timestamps, die älter als timeWindow sind
            spaceTimestamps.RemoveAll(t => now - t > timeWindow);

            if (spaceTimestamps.Count >= requiredPresses)
            {
                TryRemoveNextBiomeWall();
                spaceTimestamps.Clear();
            }
        }
    }

    private void TryRemoveNextBiomeWall()
    {
        // Hole alle ObjectManager in Szenen-Reihenfolge
        var allOM = FindObjectsOfType<ObjectManager>()
            .OrderBy(om => om.origin.y)
            .ToList();
        foreach (var om in allOM)
        {
            var wallField = om.GetType().GetField("biomeWallInstance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var wall = wallField?.GetValue(om) as GameObject;
            if (wall != null)
            {
                om.RemoveBiomeWall();
                Debug.Log($"[BiomeAccessController] Wand von {om.name} entfernt.");
                return;
            }
        }
        Debug.Log("[BiomeAccessController] Keine weitere Biome-Wand mehr vorhanden.");
    }
}