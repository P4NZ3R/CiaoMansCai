using UnityEngine;

public static class Rng
{
    static System.Random prng;

    public static void SetSeed(string seed)
    {
        prng = new System.Random(seed.GetHashCode());
    }

    public static int GetNumber(int min, int max)
    {
        if (prng == null)
        {
            Debug.LogError("ERROR: no pseudo random number generator set.");
            return 0;
        }
        return prng.Next(min, max);
    }

    public static float GetNumber(float min, float max)
    {
        return GetNumber((int)(min * 1000), (int)(max * 1000)) / 1000f;
    }
}
