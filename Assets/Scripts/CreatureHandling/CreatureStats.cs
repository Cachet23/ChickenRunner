using UnityEngine;
using System;

public class CreatureStats : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject statsUIPrefab;
    private GameObject activeUI;
    private bool isTargeted = false;

    [Header("Mana Settings")]
    [SerializeField] private float manaRegenPerSecond = 3f; // Regenerate 3 mana per second
    public bool HasEnoughMana(float amount)
    {
        return currentMana >= amount;
    }
    // Event for death notification
    public event Action<CreatureStats> OnDeath;
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackManaCost = 10f;

    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
    public float AttackManaCost => attackManaCost;
    [Header("Stats Configuration")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float maxMana = 100f;

    [Header("Stamina Settings")]
    [SerializeField] private float staminaDrainPerSecond = 10f;  // Drain 10 stamina per second while sprinting
    [SerializeField] private float staminaRegenPerSecond = 5f;   // Regenerate 5 stamina per second (10 per 2 seconds)
    [SerializeField] private float staminaRegenDelay = 1f;       // Wait 1 second after using stamina before regenerating

    private float currentHealth;
    private float currentStamina;
    private float currentMana;
    private float lastStaminaUseTime;

    // Events für UI Updates
    public event Action<float> OnHealthChanged;
    public event Action<float> OnStaminaChanged;
    public event Action<float> OnManaChanged;

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentMana = maxMana;

        // Initial events triggern
        OnHealthChanged?.Invoke(GetHealthPercent());
        OnStaminaChanged?.Invoke(GetStaminaPercent());
        OnManaChanged?.Invoke(GetManaPercent());
    }

    private void Update()
    {
        // Stamina Regeneration
        if (Time.time > lastStaminaUseTime + staminaRegenDelay)
        {
            ModifyStamina(staminaRegenPerSecond * Time.deltaTime);
        }

        // Mana Regeneration
        if (currentMana < maxMana)
        {
            ModifyMana(manaRegenPerSecond * Time.deltaTime);
        }
    }

    public void ModifyHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(GetHealthPercent());
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke(this);
        }
    }

    public void ModifyStamina(float amount)
    {
        if (amount < 0)
        {
            lastStaminaUseTime = Time.time;
        }
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        OnStaminaChanged?.Invoke(GetStaminaPercent());
    }

    public void ModifyMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        OnManaChanged?.Invoke(GetManaPercent());
    }

    public bool HasEnoughStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public float GetHealthPercent() => currentHealth / maxHealth;
    public float GetStaminaPercent() => currentStamina / maxStamina;
    public float GetManaPercent() => currentMana / maxMana;

    // Stamina beim Sprinten verbrauchen
    public void DrainStaminaForSprint()
    {
        ModifyStamina(-staminaDrainPerSecond * Time.deltaTime);
    }

    public void SetAsTarget(bool targeted)
    {
        if (isTargeted != targeted)
        {
            isTargeted = targeted;
            if (isTargeted)
            {
                ShowStatsUI();
            }
            else
            {
                HideStatsUI();
            }
        }
    }

    private void ShowStatsUI()
    {
        if (activeUI == null)
        {
            activeUI = Instantiate(statsUIPrefab, transform.position, Quaternion.identity);
            activeUI.transform.SetParent(GameObject.Find("Canvas")?.transform, true);

            // Get references to sliders
            var sliders = activeUI.GetComponentsInChildren<UnityEngine.UI.Slider>();
            foreach (var slider in sliders)
            {
                switch (slider.name.ToLower())
                {
                    case "health":
                        OnHealthChanged += (value) => slider.value = value;
                        slider.value = GetHealthPercent();
                        break;
                    case "mana":
                        OnManaChanged += (value) => slider.value = value;
                        slider.value = GetManaPercent();
                        break;
                    case "stamina":
                        OnStaminaChanged += (value) => slider.value = value;
                        slider.value = GetStaminaPercent();
                        break;
                }
            }
        }
        activeUI.SetActive(true);
        UpdateUIPosition(); // Initial position update
    }

    private void HideStatsUI()
    {
        if (activeUI != null)
        {
            // Remove all event handlers
            OnHealthChanged = null;
            OnManaChanged = null;
            OnStaminaChanged = null;
            activeUI.SetActive(false);
        }
    }

    private void UpdateUIPosition()
    {
        if (activeUI != null && Camera.main != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.5f); // Reduziert auf 0.5 Einheiten
            if (screenPos.z > 0) // Only show UI if creature is in front of camera
            {
                activeUI.transform.position = screenPos;
                activeUI.SetActive(true);
            }
            else
            {
                activeUI.SetActive(false);
            }
        }
    }

    private void LateUpdate()
    {
        if (isTargeted)
        {
            UpdateUIPosition();
        }
    }

    private void OnDestroy()
    {
        if (activeUI != null)
        {
            Destroy(activeUI);
        }
    }
}