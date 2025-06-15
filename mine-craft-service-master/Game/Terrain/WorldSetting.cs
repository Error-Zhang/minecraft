using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MineCraftService;

public enum GameMode
{
    Creative,
    Survival
}

public enum WorldMode
{
    Continent,
    Island,
    Flat
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter,
}

public class WorldSetting
{
    public string Seed { get; set; } = string.Empty;

    public WorldMode WorldMode { get; set; } = WorldMode.Continent;

    public Season Season { get; set; } = Season.Spring;

    // 生物群系大小
    public float BiomeSize { get; set; } = 1f;

    public int GetSeedToInt()
    {
        const int fnvPrime = 0x01000193;
        const int offsetBasis = unchecked((int)0x811C9DC5);
        int hash = offsetBasis;

        foreach (char c in Seed)
        {
            hash ^= c;
            hash *= fnvPrime;
        }

        return hash;
    }
}