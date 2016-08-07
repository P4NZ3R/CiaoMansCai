using UnityEngine;

public static class Rng
{
    static System.Random prng;

    static void SetSeed(string seed)
    {
        prng = new System.Random(seed.GetHashCode());
    }

    static int GetNumber(int min, int max)
    {
        if (prng == null)
        {
            Debug.LogError("ERROR: no pseudo random number generator set.");
            return 0;
        }
        return prng.Next(min, max);
    }
}
