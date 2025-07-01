using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlowerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private GameObject interactionUIPrefab;
    
    private GameObject activeUI;
    private Transform player;
    private bool isInRange;

    private void Start()
    {
        // Find the player (assuming it has the Dice tag)
        player = GameObject.FindWithTag("Dice")?.transform;
        if (player == null)
        {
            Debug.LogWarning("Player not found! Make sure it has the 'Dice' tag.");
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Check if player is in range
        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = isInRange;
        isInRange = distance <= interactionRadius;

        // Handle UI visibility
        if (isInRange != wasInRange)
        {
            if (isInRange)
            {
                ShowInteractionUI();
            }
            else
            {
                HideInteractionUI();
            }
        }

        // Handle interaction input
        if (isInRange && Input.GetKeyDown(KeyCode.E))
        {
            EatFlower();
        }

        // Update UI position if visible
        if (activeUI != null)
        {
            UpdateUIPosition();
        }
    }

    private void ShowInteractionUI()
    {
        if (activeUI == null)
        {
            activeUI = Instantiate(interactionUIPrefab, transform.position, Quaternion.identity);
            activeUI.transform.SetParent(GameObject.Find("Canvas")?.transform, true);
        }
        activeUI.SetActive(true);
    }

    private void HideInteractionUI()
    {
        if (activeUI != null)
        {
            activeUI.SetActive(false);
        }
    }

    private void UpdateUIPosition()
    {
        if (Camera.main != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
            if (screenPos.z > 0) // Only show UI if flower is in front of camera
            {
                activeUI.transform.position = screenPos;
            }
            else
            {
                activeUI.SetActive(false);
            }
        }
    }

    private void EatFlower()
    {
        // Get player stats
        var playerStats = GameObject.FindWithTag("Dice")?.GetComponent<CreatureStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("Player stats not found!");
            return;
        }

        // Get flower rarity
        var flower = GetComponent<Flower>();
        if (flower != null)
        {
            switch (flower.Rarity)
            {
                case FlowerConfig.Rarity.Common:
                    // Common flowers restore 50 stamina
                    playerStats.ModifyStamina(50f);
                    Debug.Log("Common flower eaten: +50 stamina");
                    break;

                case FlowerConfig.Rarity.Rare:
                    // Rare flowers restore 50 health
                    playerStats.ModifyHealth(50f);
                    Debug.Log("Rare flower eaten: +50 health");
                    break;

                case FlowerConfig.Rarity.Epic:
                    // Epic flowers restore all stats to max
                    playerStats.ModifyHealth(float.MaxValue); // The Clamp in ModifyHealth will handle the max value
                    playerStats.ModifyStamina(float.MaxValue); // The Clamp in ModifyStamina will handle the max value
                    playerStats.ModifyMana(float.MaxValue); // The Clamp in ModifyMana will handle the max value
                    Debug.Log("Epic flower eaten: All stats restored to max!");
                    break;
            }
        }

        // Clean up UI
        if (activeUI != null)
        {
            Destroy(activeUI);
        }
        
        // Remove the flower
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction radius in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
