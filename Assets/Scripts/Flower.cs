using UnityEngine;

public class Flower : MonoBehaviour
{
    [SerializeField] private FlowerConfig.Rarity rarity;
    
    public FlowerConfig.Rarity Rarity => rarity;
}
