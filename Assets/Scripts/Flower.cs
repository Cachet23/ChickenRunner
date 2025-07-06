using UnityEngine;

public class Flower : MonoBehaviour
{
    [SerializeField] private FlowerConfig.Rarity rarity;
    
    public FlowerConfig.Rarity Rarity => rarity;

    public void SetRarity(FlowerConfig.Rarity newRarity)
    {
        rarity = newRarity;
        Debug.Log($"[Flower] Set rarity to {newRarity} on {gameObject.name}");
    }

    // Called when the component is added to ensure we have a default rarity
    private void Awake()
    {
        Debug.Log($"[Flower] Initialized on {gameObject.name} with rarity {rarity}");
    }
}
