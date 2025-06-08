using UnityEngine;
using UnityEngine.UI;

public class CreatureStatusUI : MonoBehaviour
{
    [Header("Bar Colors")]
    [SerializeField] private Color healthColor = Color.red;
    [SerializeField] private Color staminaColor = Color.green;
    [SerializeField] private Color manaColor = Color.blue;
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Bar Settings")]
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private Vector2 barSize = new Vector2(200f, 20f);
    [SerializeField] private float barSpacing = 5f;

    private Slider healthSlider;
    private Slider staminaSlider;
    private Slider manaSlider;
    private CreatureStats stats;
    private float targetHealthFill;
    private float targetStaminaFill;
    private float targetManaFill;

    private Slider CreateStatBar(string name, Color fillColor, Vector2 anchorMinMax)
    {
        // Create the main container
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(transform, false);

        // Setup RectTransform
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        
        // Calculate the center position and width for this third
        float thirdWidth = (anchorMinMax.y - anchorMinMax.x); // Width of the third
        float centerOfThird = anchorMinMax.x + (thirdWidth / 2f); // Center point of the third
        float halfBarWidth = (barSize.x / Screen.width) / 2f; // Half of our desired bar width in normalized coordinates
        
        // Set anchors to the center 50% of the third
        sliderRect.anchorMin = new Vector2(centerOfThird - halfBarWidth, 0); // Bottom of screen
        sliderRect.anchorMax = new Vector2(centerOfThird + halfBarWidth, 0);
        sliderRect.pivot = new Vector2(0.5f, 0); // Center horizontally, bottom vertically
        sliderRect.sizeDelta = new Vector2(0, barSize.y); // Only set height, width is controlled by anchors
        sliderRect.anchoredPosition = new Vector2(0, 20); // Small margin from bottom

        // Add Slider component
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false; // Make it non-interactive

        // Create Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = backgroundColor;
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Create Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        // Create Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fillColor;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        // Setup slider references
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private void Start()
    {
        // Create the sliders - divide screen into thirds
        staminaSlider = CreateStatBar("StaminaBar", staminaColor, new Vector2(0f, 0.33f));      // Left third
        healthSlider = CreateStatBar("HealthBar", healthColor, new Vector2(0.33f, 0.66f));      // Middle third
        manaSlider = CreateStatBar("ManaBar", manaColor, new Vector2(0.66f, 1f));              // Right third

        // Find player creature by tag and get its stats
        GameObject player = GameObject.FindWithTag("Dice");
        if (player != null)
        {
            stats = player.GetComponent<CreatureStats>();
            Debug.Log($"Found player object with CreatureStats: {stats != null}");
        }
        else
        {
            Debug.LogError("Could not find object with tag 'Dice'!");
            enabled = false;
            return;
        }

        if (stats == null)
        {
            Debug.LogError("Could not find CreatureStats on player with tag 'Dice'!", this);
            enabled = false;
            return;
        }

        // Subscribe to events
        stats.OnHealthChanged += UpdateHealthBar;
        stats.OnStaminaChanged += UpdateStaminaBar;
        stats.OnManaChanged += UpdateManaBar;

        // Initialize bars with current values
        UpdateHealthBar(stats.GetHealthPercent());
        UpdateStaminaBar(stats.GetStaminaPercent());
        UpdateManaBar(stats.GetManaPercent());

        Debug.Log($"UI initialized with Sliders. Initial values - Health: {healthSlider.value}, Stamina: {staminaSlider.value}, Mana: {manaSlider.value}");
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnHealthChanged -= UpdateHealthBar;
            stats.OnStaminaChanged -= UpdateStaminaBar;
            stats.OnManaChanged -= UpdateManaBar;
        }
    }

    private void Update()
    {
        if (healthSlider == null || staminaSlider == null || manaSlider == null || stats == null)
            return;

        // Smooth value changes
        healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealthFill, Time.deltaTime * smoothSpeed);
        staminaSlider.value = Mathf.Lerp(staminaSlider.value, targetStaminaFill, Time.deltaTime * smoothSpeed);
        manaSlider.value = Mathf.Lerp(manaSlider.value, targetManaFill, Time.deltaTime * smoothSpeed);
    }

    private void UpdateHealthBar(float percent)
    {
        targetHealthFill = percent;
        Debug.Log($"Health updated to: {percent:F2}, current value: {healthSlider.value:F2}");
    }

    private void UpdateStaminaBar(float percent)
    {
        targetStaminaFill = percent;
        // Debug.Log($"Stamina updated to: {percent:F2}, current value: {staminaSlider.value:F2}");
    }

    private void UpdateManaBar(float percent)
    {
        targetManaFill = percent;
        // Debug.Log($"Mana updated to: {percent:F2}, current value: {manaSlider.value:F2}");
    }
}