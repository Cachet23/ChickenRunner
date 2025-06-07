using UnityEngine;
using UnityEngine.UI;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform dice;      // Ziehe hier das Dice-Objekt rein
    public Vector3 offset = new Vector3(0, 20, 0); // Höhe und ggf. Verschiebung anpassen
    private Vector3 initialPosition;

    [Header("Player Marker")]
    public GameObject playerMarkerPrefab; // Prefab für den Spieler-Marker
    public Color markerColor = Color.red; // Farbe des Markers
    public float markerSize = 10f;       // Größe des Markers in Pixeln
    
    private RectTransform minimapRect;    // Referenz zum Minimap RectTransform
    private RectTransform playerMarker;   // Der Marker selbst
    private Camera minimapCamera;         // Referenz zur Minimap Kamera

    void Start()
    {
        // Speichere die initiale Position
        initialPosition = transform.position;
        
        // Hole die Minimap Kamera
        minimapCamera = GetComponent<Camera>();
        
        // Finde das Minimap UI RectTransform
        var minimapUI = GameObject.Find("MinimapUI");
        if (minimapUI != null)
        {
            minimapRect = minimapUI.GetComponent<RectTransform>();
        }
        
        // Erstelle den Player Marker
        CreatePlayerMarker();
    }

    void CreatePlayerMarker()
    {
        if (playerMarkerPrefab != null)
        {
            // Instanziiere den Marker als Child des Minimap UI
            var markerObj = Instantiate(playerMarkerPrefab, minimapRect);
            playerMarker = markerObj.GetComponent<RectTransform>();
            
            // Setze die Eigenschaften des Markers
            playerMarker.sizeDelta = new Vector2(markerSize, markerSize);
            var image = markerObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = markerColor;
            }
        }
        else
        {
            // Erstelle einen einfachen Marker falls kein Prefab vorhanden
            var markerObj = new GameObject("PlayerMarker");
            markerObj.transform.SetParent(minimapRect, false);
            playerMarker = markerObj.AddComponent<RectTransform>();
            playerMarker.sizeDelta = new Vector2(markerSize, markerSize);
            
            var image = markerObj.AddComponent<Image>();
            image.color = markerColor;
        }
    }

    void LateUpdate()
    {
        if (dice != null)
        {
            // Behalte x und y von der initialen Position, update nur z vom Würfel
            Vector3 newPosition = initialPosition;
            newPosition.z = dice.position.z + offset.z;
            transform.position = newPosition;

            // Update die Position des Markers
            if (playerMarker != null && minimapCamera != null && minimapRect != null)
            {
                // Konvertiere die Weltposition des Spielers in Viewport-Koordinaten
                Vector3 viewportPoint = minimapCamera.WorldToViewportPoint(dice.position);
                
                // Konvertiere Viewport-Koordinaten in lokale UI-Koordinaten
                Vector2 screenPoint = new Vector2(
                    (viewportPoint.x * minimapRect.sizeDelta.x) - (minimapRect.sizeDelta.x * 0.5f),
                    (viewportPoint.z * minimapRect.sizeDelta.y) - (minimapRect.sizeDelta.y * 0.5f)
                );
                
                playerMarker.anchoredPosition = screenPoint;
            }
        }
    }
}