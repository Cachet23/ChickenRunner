using UnityEngine;

[System.Serializable]
public class FlowerConfig
{
    public GameObject flowerPrefab;
    public enum Rarity { Common, Rare, Epic }
    public Rarity rarity;
    [Range(0f, 1f)]
    public float replacementChance = 0.2f; // Chance to replace grass with this flower
}
