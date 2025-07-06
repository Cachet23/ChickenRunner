using UnityEngine;

[System.Serializable]
public class FlowerConfig
{
    public GameObject flowerPrefab;
    public enum Rarity { Common, Rare, Epic }

    // Hardcoded replacement chances for each rarity
    public const float COMMON_REPLACE_CHANCE = 0.3f;
    public const float RARE_REPLACE_CHANCE = 0.15f;
    public const float EPIC_REPLACE_CHANCE = 0.05f;

    public static float GetReplacementChance(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => COMMON_REPLACE_CHANCE,
            Rarity.Rare => RARE_REPLACE_CHANCE,
            Rarity.Epic => EPIC_REPLACE_CHANCE,
            _ => COMMON_REPLACE_CHANCE
        };
    }
}
