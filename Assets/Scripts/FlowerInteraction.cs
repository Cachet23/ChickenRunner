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
