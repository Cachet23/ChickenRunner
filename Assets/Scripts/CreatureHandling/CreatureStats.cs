using UnityEngine;
using System;
using System.Linq;

public class CreatureStats : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject statsUIPrefab;
    private GameObject activeUI;
    private bool isTargeted = false;
    private float lastCooldownUpdate = 0f;
    private const float COOLDOWN_UPDATE_INTERVAL = 0.05f; // Update alle 0.05 Sekunden für smoothe Animation

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
    [SerializeField] private float attackCooldown = 0.75f; // Zeit in Sekunden zwischen Attacken
    private float nextAttackTime = 0f;

    // UI & Targeting nur für Player relevant
    [SerializeField] private float sightRange = 5f;
    public float SightRange => CompareTag("Dice") ? sightRange : 0f;
    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
    public float AttackManaCost => attackManaCost;

    // Prüft ob die Creature attackieren kann (Cooldown & Mana)
    public bool CanAttack()
    {
        bool isReady = Time.time >= nextAttackTime;
        bool hasMana = HasEnoughMana(attackManaCost);
        return isReady && hasMana;
    }

    // Prüft ob ein Target in Range ist
    public bool IsInAttackRange(CreatureStats target)
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.transform.position) <= attackRange;
    }

    // Zentrale Attack-Methode
    public bool TryAttack(CreatureStats target)
    {
        if (!CanAttack() || !IsInAttackRange(target))
        {
            return false;
        }

        // Führe Attacke aus
        target.ModifyHealth(-attackDamage);
        ModifyMana(-attackManaCost);
        
        // Setze Cooldown
        nextAttackTime = Time.time + attackCooldown;
        OnCooldownProgress?.Invoke(0f); // Signal cooldown start (0 = gerade benutzt)
        
        Debug.Log($"[CreatureStats] {gameObject.name} attacked {target.gameObject.name} for {attackDamage} damage");
        return true;
    }
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
    public event Action<float> OnCooldownProgress; // 0 = ready, 1 = just started cooldown

    public float GetCooldownProgress()
    {
        if (Time.time >= nextAttackTime) return 1f; // Voll gefüllt wenn Attacke bereit
        float remainingTime = nextAttackTime - Time.time;
        return 1f - (remainingTime / attackCooldown); // Von 0 (gerade benutzt) bis 1 (bereit)
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentMana = maxMana;
        lastCooldownUpdate = Time.time;

        // Initial events triggern
        OnHealthChanged?.Invoke(GetHealthPercent());
        OnStaminaChanged?.Invoke(GetStaminaPercent());
        OnManaChanged?.Invoke(GetManaPercent());
    }

    private void Update()
    {
        // Nur der Player soll regenerieren
        if (!CompareTag("Dice")) return;

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

        // Update cooldown progress if targeted
        if (isTargeted && Time.time >= lastCooldownUpdate + COOLDOWN_UPDATE_INTERVAL)
        {
            lastCooldownUpdate = Time.time;
            float progress = GetCooldownProgress();
            OnCooldownProgress?.Invoke(progress);
            Debug.Log($"[CreatureStats] {gameObject.name} Cooldown Update: {progress:F2}");
        }
    }

    public void ModifyHealth(float amount)
    {
        float oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log($"[CreatureStats] {gameObject.name} health modified: {oldHealth} -> {currentHealth} (amount: {amount})");
        OnHealthChanged?.Invoke(GetHealthPercent());
        if (currentHealth <= 0)
        {
            Debug.Log($"[CreatureStats] {gameObject.name} died!");
            OnDeath?.Invoke(this);
        }
    }

    public void ModifyStamina(float amount)
    {
        float oldStamina = currentStamina;
        if (amount < 0)
        {
            lastStaminaUseTime = Time.time;
        }
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        Debug.Log($"[CreatureStats] {gameObject.name} stamina modified: {oldStamina} -> {currentStamina} (amount: {amount})");
        OnStaminaChanged?.Invoke(GetStaminaPercent());
    }

    public void ModifyMana(float amount)
    {
        float oldMana = currentMana;
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        Debug.Log($"[CreatureStats] {gameObject.name} mana modified: {oldMana} -> {currentMana} (amount: {amount})");
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
        if (activeUI == null && statsUIPrefab != null)
        {
            Debug.Log($"[CreatureStats] Creating UI for {gameObject.name}");
            activeUI = Instantiate(statsUIPrefab, transform.position, Quaternion.identity);
            activeUI.transform.SetParent(GameObject.Find("Canvas")?.transform, true);

            // Get references to sliders and cooldown image
            var sliders = activeUI.GetComponentsInChildren<UnityEngine.UI.Slider>();
            var allImages = activeUI.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            var cooldownImage = allImages.FirstOrDefault(img => img.gameObject.name.ToLower().Contains("cooldown"));

            Debug.Log($"[CreatureStats] Found {sliders.Length} sliders and {allImages.Length} images");
            foreach (var slider in sliders)
            {
                Debug.Log($"[CreatureStats] Found slider: {slider.name}");
                switch (slider.name.ToLower())
                {
                    case "health":
                        OnHealthChanged += (value) => {
                            slider.value = value;
                            Debug.Log($"[CreatureStats] Health updated to {value:F2}");
                        };
                        slider.value = GetHealthPercent();
                        break;
                    case "mana":
                        OnManaChanged += (value) => {
                            slider.value = value;
                            Debug.Log($"[CreatureStats] Mana updated to {value:F2}");
                        };
                        slider.value = GetManaPercent();
                        break;
                    case "stamina":
                        OnStaminaChanged += (value) => {
                            slider.value = value;
                            Debug.Log($"[CreatureStats] Stamina updated to {value:F2}");
                        };
                        slider.value = GetStaminaPercent();
                        break;
                }
            }

            // Setup cooldown UI if found
            if (cooldownImage != null && cooldownImage.name.ToLower().Contains("cooldown"))
            {
                cooldownImage.type = UnityEngine.UI.Image.Type.Filled;
                cooldownImage.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
                cooldownImage.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Top;
                cooldownImage.fillClockwise = true;
                cooldownImage.fillAmount = 1f; // Start ready
                cooldownImage.gameObject.SetActive(true);

                OnCooldownProgress += (progress) => {
                    if (cooldownImage != null)
                    {
                        cooldownImage.fillAmount = progress;
                        Debug.Log($"[CreatureStats] {gameObject.name} cooldown progress: {progress:F2}, Time: {Time.time:F2}, NextAttack: {nextAttackTime:F2}");
                    }
                };
                Debug.Log($"[CreatureStats] Found and initialized cooldown UI for {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"[CreatureStats] No cooldown UI image found in {gameObject.name}!");
            }
        }
        else if (statsUIPrefab == null)
        {
            Debug.LogError($"[CreatureStats] statsUIPrefab is not assigned on {gameObject.name}!");
            return;
        }

        if (activeUI != null)
        {
            activeUI.SetActive(true);
            UpdateUIPosition(); // Initial position update
        }
    }

    private void HideStatsUI()
    {
        if (activeUI != null)
        {
            // Nur UI ausblenden, Event-Handler beibehalten
            activeUI.SetActive(false);
        }
    }

    // Event-Handler nur beim wirklichen Zerstören des UIs entfernen
    private void CleanupStatsUI()
    {
        if (activeUI != null)
        {
            OnHealthChanged = null;
            OnManaChanged = null;
            OnStaminaChanged = null;
            OnCooldownProgress = null;
            Destroy(activeUI);
            activeUI = null;
        }
    }

    private void UpdateUIPosition()
    {
        if (activeUI != null && Camera.main != null)
        {
            var screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.6f);
            
            // Check if creature is in front of camera
            if (screenPos.z > 0)
            {
                activeUI.transform.position = screenPos;
                
                // Update UI color based on distance to player
                var playerTransform = GameObject.FindWithTag("Dice")?.transform;
                if (playerTransform != null)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                    bool inAttackRange = distanceToPlayer <= attackRange;
                    
                    // Get UI text component and update color based on range
                    var text = activeUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (text != null)
                    {
                        if (inAttackRange)
                        {
                            text.color = Color.red;    // In attack range - red
                            text.text = "In Attack Range!";
                        }
                        else
                        {
                            text.color = Color.yellow; // In sight but not in attack range - yellow
                            text.text = "Target Spotted";
                        }
                    }
                }
            }
            else
            {
                // Hide UI when behind camera
                activeUI.transform.position = new Vector3(-1000, -1000, -1000);
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
        CleanupStatsUI();
    }

    // Properties für Max-Werte
    public float MaxHealth => maxHealth;
    public float MaxStamina => maxStamina;
    public float MaxMana => maxMana;

    // Restore-Methoden
    public void RestoreHealth(float amount)
    {
        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (currentHealth != oldHealth)
        {
            OnHealthChanged?.Invoke(currentHealth / maxHealth);
            Debug.Log($"[CreatureStats] Restored {currentHealth - oldHealth} health to {gameObject.name}");
        }
    }

    public void RestoreStamina(float amount)
    {
        float oldStamina = currentStamina;
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        if (currentStamina != oldStamina)
        {
            OnStaminaChanged?.Invoke(currentStamina / maxStamina);
            Debug.Log($"[CreatureStats] Restored {currentStamina - oldStamina} stamina to {gameObject.name}");
        }
    }

    public void RestoreMana(float amount)
    {
        float oldMana = currentMana;
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        if (currentMana != oldMana)
        {
            OnManaChanged?.Invoke(currentMana / maxMana);
            Debug.Log($"[CreatureStats] Restored {currentMana - oldMana} mana to {gameObject.name}");
        }
    }
}