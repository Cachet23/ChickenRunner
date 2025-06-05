using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public Camera minimapCamera;
    public RawImage minimapImage;
    public RectTransform minimapRect;
    public Vector2Int mapSize = new Vector2Int(100, 100);
    public float border = 10f;

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
    }
}
