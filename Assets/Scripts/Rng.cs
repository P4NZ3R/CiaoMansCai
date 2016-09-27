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

    public static Vector2 GetVector2FromAngle(float angleDegrees, float radius = 1)
    {
        float x;
        float y;
        float angleRadians;
        Vector2 vector;
        // convert degrees to radians
        angleRadians = angleDegrees * Mathf.PI / 180.0f;
        // get the 2D dimensional coordinates
        x = radius * Mathf.Cos(angleRadians);
        y = radius * Mathf.Sin(angleRadians);
        // derive the 2D vector
        vector = new Vector2(x, y);
        // return the vector info
        return vector;
    }

    public static Vector2 GetVersor2FromAngle(float angleDegrees)
    {
        return GetVector2FromAngle(angleDegrees);
    }

    public static Vector2 GetRandomVersor()
    {
        float randomAngle = Rng.GetNumber(0f, Mathf.PI * 2f);
        return new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle)).normalized;
    }

    public static Vector2 ApplyInaccuracy(Vector2 vector, float spreadFactor)
    {
        return (Rng.GetRandomVersor() * spreadFactor + vector).normalized;
    }
}
