using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public Camera minimapCamera;
    public RawImage minimapImage;
    public RectTransform minimapRect;
    public Vector2Int mapSize = new Vector2Int(100, 100);
    public float border = 10f;

    [Header("Player Marker")]
    public Transform playerTransform;
    public float markerSize = 10f;
    public Color markerColor = Color.red;
    private RectTransform playerMarker;

    void Start()
    {
        if (minimapCamera != null)
        {
            // Set camera size and position to fit the whole map
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = mapSize.y / 2f + border;
            minimapCamera.transform.position = new Vector3(mapSize.x / 2f, mapSize.y / 2f, -20f);
            minimapCamera.cullingMask = LayerMask.GetMask("Default", "Props");
        }

        // Find player if not set
        if (playerTransform == null)
        {
            var player = GameObject.FindWithTag("Dice");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        CreatePlayerMarker();
    }

    void CreatePlayerMarker()
    {
        // Create a simple marker
        var markerObj = new GameObject("PlayerMarker");
        markerObj.transform.SetParent(minimapRect, false);
        playerMarker = markerObj.AddComponent<RectTransform>();
        playerMarker.sizeDelta = new Vector2(markerSize, markerSize);
        
        // Add and configure the image component
        var image = markerObj.AddComponent<UnityEngine.UI.Image>();
        image.color = markerColor;
        
        // Make the image circular using the built-in circle sprite
        var texture = new Texture2D(128, 128);
        var colors = new Color[texture.width * texture.height];
        
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                var dx = x - texture.width / 2f;
                var dy = y - texture.height / 2f;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                var radius = texture.width / 2f;
                colors[y * texture.width + x] = dist <= radius ? Color.white : Color.clear;
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
    }

    void LateUpdate()
    {
        if (playerTransform != null && playerMarker != null && minimapCamera != null)
        {
            // Convert world position to screen position
            Vector3 screenPoint = minimapCamera.WorldToViewportPoint(playerTransform.position);
            
            // Convert viewport position to UI position
            Vector2 uiPosition = new Vector2(
                (screenPoint.x * minimapRect.sizeDelta.x) - (minimapRect.sizeDelta.x * 0.5f),
                (screenPoint.y * minimapRect.sizeDelta.y) - (minimapRect.sizeDelta.y * 0.5f)
            );
            
            playerMarker.anchoredPosition = uiPosition;
        }
    }
}
