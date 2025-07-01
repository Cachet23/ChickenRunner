using UnityEngine;
using System;

public class CreatureStats : MonoBehaviour
{
    // Event for death notification
    public event Action<CreatureStats> OnDeath;
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackDamage = 20f;

    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
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
}